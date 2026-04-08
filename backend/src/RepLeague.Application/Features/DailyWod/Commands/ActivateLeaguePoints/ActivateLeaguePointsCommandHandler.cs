using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.DailyWod.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.DailyWod.Commands.ActivateLeaguePoints;

public class ActivateLeaguePointsCommandHandler(IAppDbContext db)
    : IRequestHandler<ActivateLeaguePointsCommand, ActivateLeaguePointsResult>
{
    public async Task<ActivateLeaguePointsResult> Handle(
        ActivateLeaguePointsCommand request, CancellationToken ct)
    {
        var league = await db.Leagues
            .FirstOrDefaultAsync(l => l.Id == request.LeagueId, ct)
            ?? throw new NotFoundException(nameof(League), request.LeagueId);

        if (league.OwnerUserId != request.RequestingUserId)
            throw new UnauthorizedException("Solo el owner de la liga puede activar el sistema de puntos.");

        if (league.PointsActivatedAt.HasValue)
            throw new ConflictException("El sistema de puntos ya está activado en esta liga.");

        league.PointsActivatedAt = DateOnly.FromDateTime(DateTime.UtcNow);

        int backfilledDays = 0;

        if (request.RunBackfill)
        {
            backfilledDays = await RunBackfillAsync(league, request.RequestingUserId, ct);
            league.BackfillCompleted = true;
        }

        await db.SaveChangesAsync(ct);

        return new ActivateLeaguePointsResult(league.PointsActivatedAt.Value, backfilledDays);
    }

    private async Task<int> RunBackfillAsync(
        Domain.Entities.League league, Guid userId, CancellationToken ct)
    {
        // Get all members of the league
        var memberIds = await db.LeagueMembers
            .Where(m => m.LeagueId == league.Id)
            .Select(m => m.UserId)
            .ToListAsync(ct);

        var totalBackfilled = 0;

        foreach (var memberId in memberIds)
        {
            // Get all sessions for this member from league creation date
            var sessionDates = await db.LiftSessions
                .Where(s => s.UserId == memberId && s.Date >= DateOnly.FromDateTime(league.CreatedAt) && !s.IsDeleted)
                .Select(s => s.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync(ct);

            var allSessionDatesSorted = sessionDates.ToList();

            for (int i = 0; i < allSessionDatesSorted.Count; i++)
            {
                var date = allSessionDatesSorted[i];

                // AttendancePoints = 1 always
                var attendance = 1;

                // VolumePoints: volume today vs average of previous days
                var todayVolume = await db.StrengthSets
                    .Where(s => s.LiftSession.UserId == memberId
                             && s.LiftSession.Date == date
                             && !s.LiftSession.IsDeleted)
                    .SumAsync(s => (decimal?)(s.Reps * s.WeightKg), ct) ?? 0m;

                var previousDates = allSessionDatesSorted.Take(i).ToList();
                int volumePoints = 0;
                if (previousDates.Count > 0)
                {
                    var prevVolumes = new List<decimal>();
                    foreach (var pd in previousDates)
                    {
                        var vol = await db.StrengthSets
                            .Where(s => s.LiftSession.UserId == memberId
                                     && s.LiftSession.Date == pd
                                     && !s.LiftSession.IsDeleted)
                            .SumAsync(s => (decimal?)(s.Reps * s.WeightKg), ct) ?? 0m;
                        prevVolumes.Add(vol);
                    }
                    var avgVolume = prevVolumes.Average();
                    volumePoints = todayVolume > avgVolume ? 1 : 0;
                }

                // PrPoints: any PR set on this day
                var hasPr = await db.StrengthSets
                    .AnyAsync(s => s.LiftSession.UserId == memberId
                                && s.LiftSession.Date == date
                                && !s.LiftSession.IsDeleted
                                && s.IsPr, ct);
                var prPoints = hasPr ? 2 : 0;

                // StreakPoints: 3+ consecutive days
                var streakPoints = 0;
                if (i >= 2)
                {
                    var prevDay1 = allSessionDatesSorted[i - 1];
                    var prevDay2 = allSessionDatesSorted[i - 2];
                    if (date.DayNumber - prevDay1.DayNumber == 1 &&
                        prevDay1.DayNumber - prevDay2.DayNumber == 1)
                    {
                        streakPoints = 2;
                    }
                }

                // Upsert DailyPoints
                var existing = await db.DailyPoints
                    .FirstOrDefaultAsync(p => p.UserId == memberId
                                           && p.LeagueId == league.Id
                                           && p.Date == date, ct);

                if (existing == null)
                {
                    db.DailyPoints.Add(new Domain.Entities.DailyPoints
                    {
                        Id = Guid.NewGuid(),
                        UserId = memberId,
                        LeagueId = league.Id,
                        Date = date,
                        AttendancePoints = attendance,
                        VolumePoints = volumePoints,
                        PrPoints = prPoints,
                        WodCompletionPoints = 0,
                        WodRankingPoints = 0,
                        StreakPoints = streakPoints,
                        CalculatedAt = DateTime.UtcNow
                    });
                    totalBackfilled++;
                }
                else
                {
                    existing.AttendancePoints = attendance;
                    existing.VolumePoints = volumePoints;
                    existing.PrPoints = prPoints;
                    existing.StreakPoints = streakPoints;
                    existing.CalculatedAt = DateTime.UtcNow;
                }
            }
        }

        return totalBackfilled;
    }
}
