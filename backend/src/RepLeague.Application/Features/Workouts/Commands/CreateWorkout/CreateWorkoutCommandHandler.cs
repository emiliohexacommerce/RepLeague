using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Workouts.DTOs;
using RepLeague.Domain.Entities;
using RepLeague.Domain.Enums;

namespace RepLeague.Application.Features.Workouts.Commands.CreateWorkout;

public class CreateWorkoutCommandHandler(IAppDbContext db, IEmailService emailService)
    : IRequestHandler<CreateWorkoutCommand, WorkoutDto>
{
    private const int PointsPerWorkout = 10;
    private const int PointsPerPr = 30;

    private record PrDetail(string MovementName, string PreviousValue, string NewValue);

    public async Task<WorkoutDto> Handle(CreateWorkoutCommand request, CancellationToken ct)
    {
        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = request.Type,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        // ── Build exercises / WOD ─────────────────────────────────────────────
        if (request.Type == WorkoutType.Strength && request.Exercises != null)
        {
            workout.Exercises = request.Exercises.Select(e => new WorkoutExercise
            {
                Id = Guid.NewGuid(),
                WorkoutId = workout.Id,
                ExerciseName = e.ExerciseName.Trim(),
                Sets = e.Sets,
                Reps = e.Reps,
                WeightKg = e.WeightKg
            }).ToList();
        }

        if (request.Type == WorkoutType.WOD && request.Wod != null)
        {
            workout.Wod = new WorkoutWod
            {
                Id = Guid.NewGuid(),
                WorkoutId = workout.Id,
                WodName = request.Wod.WodName.Trim(),
                Duration = ParseDuration(request.Wod.Duration),
                Rounds = request.Wod.Rounds,
                TotalReps = request.Wod.TotalReps
            };
        }

        // ── PR Detection ──────────────────────────────────────────────────────
        var prDetail = await DetectPrDetailsAsync(workout, request.UserId, ct);
        workout.IsPR = prDetail != null;

        db.Workouts.Add(workout);

        // ── Ranking update (incremental) ──────────────────────────────────────
        await UpdateRankingAsync(request.UserId, workout.IsPR, ct);

        await db.SaveChangesAsync(ct);

        // ── PR email (fire-and-forget) ────────────────────────────────────────
        if (prDetail != null)
            _ = SendPrEmailAsync(request.UserId, prDetail, ct);

        return ToDto(workout);
    }

    // ── PR Detection ──────────────────────────────────────────────────────────

    private async Task<PrDetail?> DetectPrDetailsAsync(Workout workout, Guid userId, CancellationToken ct)
    {
        if (workout.Type == WorkoutType.Strength)
            return await DetectStrengthPrDetailsAsync(workout.Exercises, userId, ct);

        if (workout.Type == WorkoutType.WOD && workout.Wod?.Duration != null)
            return await DetectWodPrDetailsAsync(workout.Wod, userId, ct);

        return null;
    }

    private async Task<PrDetail?> DetectStrengthPrDetailsAsync(
        ICollection<WorkoutExercise> exercises, Guid userId, CancellationToken ct)
    {
        foreach (var exercise in exercises)
        {
            var maxPrevWeight = await db.WorkoutExercises
                .Where(e => e.ExerciseName == exercise.ExerciseName
                         && e.Workout.UserId == userId)
                .MaxAsync(e => (decimal?)e.WeightKg, ct);

            if (maxPrevWeight == null || exercise.WeightKg > maxPrevWeight)
            {
                var prev = maxPrevWeight.HasValue ? $"{maxPrevWeight:0.##} kg" : "Primera vez";
                return new PrDetail(exercise.ExerciseName, prev, $"{exercise.WeightKg:0.##} kg");
            }
        }
        return null;
    }

    private async Task<PrDetail?> DetectWodPrDetailsAsync(WorkoutWod wod, Guid userId, CancellationToken ct)
    {
        var minPrevDuration = await db.WorkoutWods
            .Where(w => w.WodName == wod.WodName
                     && w.Duration != null
                     && w.Workout.UserId == userId)
            .MinAsync(w => (TimeSpan?)w.Duration, ct);

        if (minPrevDuration == null || wod.Duration < minPrevDuration)
        {
            var prev = minPrevDuration.HasValue
                ? minPrevDuration.Value.ToString(@"mm\:ss")
                : "Primera vez";
            return new PrDetail(wod.WodName, prev, wod.Duration!.Value.ToString(@"mm\:ss"));
        }
        return null;
    }

    // ── PR email ──────────────────────────────────────────────────────────────

    private async Task SendPrEmailAsync(Guid userId, PrDetail pr, CancellationToken ct)
    {
        try
        {
            var user = await db.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.Email, u.DisplayName })
                .FirstOrDefaultAsync(ct);

            if (user == null) return;

            var firstName = user.DisplayName.Split(' ')[0];
            await emailService.SendNewPrEmailAsync(
                user.Email, firstName, pr.MovementName, pr.PreviousValue, pr.NewValue, ct);
        }
        catch
        {
            // Email failure must not affect the workout save
        }
    }

    // ── Ranking update ────────────────────────────────────────────────────────

    private async Task UpdateRankingAsync(Guid userId, bool isPr, CancellationToken ct)
    {
        var leagueIds = await db.LeagueMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.LeagueId)
            .ToListAsync(ct);

        if (leagueIds.Count == 0) return;

        var entries = await db.RankingEntries
            .Where(r => r.UserId == userId && leagueIds.Contains(r.LeagueId))
            .ToListAsync(ct);

        var pointsToAdd = PointsPerWorkout + (isPr ? PointsPerPr : 0);

        foreach (var leagueId in leagueIds)
        {
            var entry = entries.FirstOrDefault(e => e.LeagueId == leagueId);

            if (entry == null)
            {
                entry = new RankingEntry
                {
                    Id = Guid.NewGuid(),
                    LeagueId = leagueId,
                    UserId = userId,
                    Points = 0,
                    WorkoutCount = 0,
                    PrCount = 0
                };
                db.RankingEntries.Add(entry);
            }

            entry.Points += pointsToAdd;
            entry.WorkoutCount += 1;
            if (isPr) entry.PrCount += 1;
            entry.UpdatedAt = DateTime.UtcNow;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TimeSpan? ParseDuration(string? duration)
    {
        if (string.IsNullOrWhiteSpace(duration)) return null;
        return TimeSpan.TryParse(duration, out var ts) ? ts : null;
    }

    private static WorkoutDto ToDto(Workout w) => new(
        w.Id,
        w.Type,
        w.IsPR,
        w.Notes,
        w.CreatedAt,
        w.Exercises.Select(e => new ExerciseDto(e.Id, e.ExerciseName, e.Sets, e.Reps, e.WeightKg)).ToList(),
        w.Wod == null ? null : new WodDto(
            w.Wod.WodName,
            w.Wod.Duration?.ToString(@"hh\:mm\:ss"),
            w.Wod.Rounds,
            w.Wod.TotalReps)
    );
}
