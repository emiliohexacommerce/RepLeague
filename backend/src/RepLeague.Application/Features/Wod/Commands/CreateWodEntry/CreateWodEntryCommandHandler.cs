using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Common.Utils;
using RepLeague.Application.Features.Wod.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Wod.Commands.CreateWodEntry;

public class CreateWodEntryCommandHandler(IAppDbContext db)
    : IRequestHandler<CreateWodEntryCommand, WodEntryDto>
{
    public async Task<WodEntryDto> Handle(CreateWodEntryCommand request, CancellationToken ct)
    {
        var entry = new WodEntry
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title?.Trim(),
            Date = request.Date,
            TimeCapSeconds = TimeParser.ParseToSeconds(request.TimeCap),
            ElapsedSeconds = TimeParser.ParseToSeconds(request.ElapsedTime),
            Rounds = request.Rounds,
            RxScaled = request.RxScaled,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        entry.Exercises = request.Exercises
            .Select(e => new WodExercise
            {
                Id = Guid.NewGuid(),
                WodEntryId = entry.Id,
                OrderIndex = e.OrderIndex,
                Name = e.Name.Trim(),
                MovementType = e.MovementType,
                LoadValue = e.LoadValue,
                LoadUnit = e.LoadUnit,
                Reps = e.Reps,
                Notes = e.Notes?.Trim()
            }).ToList();

        if (request.AmrapResult != null)
        {
            entry.AmrapResult = new WodResultAmrap
            {
                WodEntryId = entry.Id,
                RoundsCompleted = request.AmrapResult.RoundsCompleted,
                ExtraReps = request.AmrapResult.ExtraReps
            };
        }

        if (request.EmomResult != null)
        {
            entry.EmomResult = new WodResultEmom
            {
                WodEntryId = entry.Id,
                TotalMinutes = request.EmomResult.TotalMinutes,
                IntervalsDone = request.EmomResult.IntervalsDone
            };
        }

        db.WodEntries.Add(entry);
        await UpdateRankingAsync(request.UserId, ct);
        await db.SaveChangesAsync(ct);

        return ToDto(entry);
    }

    private async Task UpdateRankingAsync(Guid userId, CancellationToken ct)
    {
        const int PointsPerWorkout = 10;

        var leagueIds = await db.LeagueMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.LeagueId)
            .ToListAsync(ct);

        if (leagueIds.Count == 0) return;

        var entries = await db.RankingEntries
            .Where(r => r.UserId == userId && leagueIds.Contains(r.LeagueId))
            .ToListAsync(ct);

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

            entry.Points += PointsPerWorkout;
            entry.WorkoutCount += 1;
            entry.UpdatedAt = DateTime.UtcNow;
        }
    }

    internal static WodEntryDto ToDto(WodEntry e) => new(
        e.Id,
        e.Type,
        e.Title,
        e.Date,
        TimeParser.FormatSeconds(e.TimeCapSeconds),
        TimeParser.FormatSeconds(e.ElapsedSeconds),
        e.Rounds,
        e.RxScaled,
        e.Notes,
        e.CreatedAt,
        e.Exercises
            .OrderBy(x => x.OrderIndex)
            .Select(x => new WodExerciseDto(x.Id, x.OrderIndex, x.Name, x.MovementType, x.LoadValue, x.LoadUnit, x.Reps, x.Notes))
            .ToList(),
        e.AmrapResult == null ? null : new WodResultAmrapDto(e.AmrapResult.RoundsCompleted, e.AmrapResult.ExtraReps),
        e.EmomResult == null ? null : new WodResultEmomDto(e.EmomResult.TotalMinutes, e.EmomResult.IntervalsDone)
    );
}
