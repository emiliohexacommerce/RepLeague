using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Points.Commands.CalculateDailyPoints;
using RepLeague.Application.Features.Strength.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Strength.Commands.CreateLiftSession;

public class CreateLiftSessionCommandHandler(IAppDbContext db, IMediator mediator)
    : IRequestHandler<CreateLiftSessionCommand, LiftSessionDto>
{
    public async Task<LiftSessionDto> Handle(CreateLiftSessionCommand request, CancellationToken ct)
    {
        var oneRmMethod = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => u.OneRmMethod)
            .FirstOrDefaultAsync(ct) ?? "Epley";

        var session = new LiftSession
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Date = request.Date,
            Title = request.Title?.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        // Build sets with PR detection and 1RM calculation
        var sets = new List<StrengthSet>();
        foreach (var s in request.Sets)
        {
            var oneRm = s.Reps > 0 && s.WeightKg > 0 && !s.IsWarmup
                ? ComputeOneRm(s.WeightKg, s.Reps, oneRmMethod)
                : (decimal?)null;

            var isPr = false;
            if (!s.IsWarmup && oneRm.HasValue)
            {
                var prevBest = await db.StrengthSets
                    .Where(x => x.LiftSession.UserId == request.UserId
                             && x.ExerciseName == s.ExerciseName.Trim()
                             && !x.IsWarmup
                             && x.OneRepMaxKg.HasValue)
                    .MaxAsync(x => (decimal?)x.OneRepMaxKg, ct);

                isPr = prevBest == null || oneRm > prevBest;
            }

            sets.Add(new StrengthSet
            {
                Id = Guid.NewGuid(),
                LiftSessionId = session.Id,
                ExerciseName = s.ExerciseName.Trim(),
                SetNumber = s.SetNumber,
                Reps = s.Reps,
                WeightKg = s.WeightKg,
                IsWarmup = s.IsWarmup,
                IsPr = isPr,
                OneRepMaxKg = oneRm,
                Notes = s.Notes?.Trim()
            });
        }

        session.Sets = sets;
        db.LiftSessions.Add(session);

        var sessionHasPr = sets.Any(s => s.IsPr);
        await UpdateRankingAsync(request.UserId, sessionHasPr, ct);

        await db.SaveChangesAsync(ct);

        // Trigger daily points calculation for all leagues with points activated
        await TriggerDailyPointsAsync(request.UserId, request.Date, ct);

        return ToDto(session);
    }

    private async Task TriggerDailyPointsAsync(Guid userId, DateOnly date, CancellationToken ct)
    {
        var activatedLeagueIds = await db.LeagueMembers
            .Where(m => m.UserId == userId && m.League.PointsActivatedAt.HasValue)
            .Select(m => m.LeagueId)
            .ToListAsync(ct);

        foreach (var leagueId in activatedLeagueIds)
        {
            await mediator.Send(new CalculateDailyPointsCommand(userId, leagueId, date), ct);
        }
    }

    private async Task UpdateRankingAsync(Guid userId, bool isPr, CancellationToken ct)
    {
        const int PointsPerWorkout = 10;
        const int PointsPerPr = 30;

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

    /// <summary>Epley: w × (1 + reps/30) · Brzycki: w × 36/(37-reps)</summary>
    private static decimal ComputeOneRm(decimal weight, int reps, string method)
    {
        if (method == "Brzycki" && reps > 0 && reps < 37)
            return Math.Round(weight * 36m / (37m - reps), 1);
        return Math.Round(weight * (1m + reps / 30m), 1);
    }

    internal static LiftSessionDto ToDto(LiftSession s) => new(
        s.Id,
        s.Date,
        s.Title,
        s.Notes,
        s.CreatedAt,
        s.Sets.OrderBy(x => x.ExerciseName).ThenBy(x => x.SetNumber)
            .Select(x => new StrengthSetDto(
                x.Id, x.ExerciseName, x.SetNumber, x.Reps, x.WeightKg,
                x.IsWarmup, x.IsPr, x.OneRepMaxKg, x.Notes))
            .ToList()
    );
}
