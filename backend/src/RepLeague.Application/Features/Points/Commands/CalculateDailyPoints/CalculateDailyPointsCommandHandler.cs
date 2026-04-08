using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Points.Commands.CalculateDailyPoints;

public class CalculateDailyPointsCommandHandler(IAppDbContext db)
    : IRequestHandler<CalculateDailyPointsCommand, Unit>
{
    public async Task<Unit> Handle(CalculateDailyPointsCommand request, CancellationToken ct)
    {
        var userId = request.UserId;
        var leagueId = request.LeagueId;
        var date = request.Date;

        // 1. AttendancePoints (+1): at least one LiftSession or WodEntry that day
        var hasLiftSession = await db.LiftSessions
            .AnyAsync(s => s.UserId == userId && s.Date == date && !s.IsDeleted, ct);
        var hasWodEntry = await db.WodEntries
            .AnyAsync(w => w.UserId == userId && w.Date == date && !w.IsDeleted, ct);
        var attendancePoints = (hasLiftSession || hasWodEntry) ? 1 : 0;

        // 2. VolumePoints (+1): today's volume > historical average
        var todayVolume = await db.StrengthSets
            .Where(s => s.LiftSession.UserId == userId
                     && s.LiftSession.Date == date
                     && !s.LiftSession.IsDeleted)
            .SumAsync(s => (decimal?)(s.Reps * s.WeightKg), ct) ?? 0m;

        var previousVolumes = await db.StrengthSets
            .Where(s => s.LiftSession.UserId == userId
                     && s.LiftSession.Date < date
                     && !s.LiftSession.IsDeleted)
            .GroupBy(s => s.LiftSession.Date)
            .Select(g => g.Sum(s => (decimal)(s.Reps * s.WeightKg)))
            .ToListAsync(ct);

        int volumePoints = 0;
        if (previousVolumes.Count > 0)
        {
            var avgVolume = previousVolumes.Average();
            volumePoints = todayVolume > avgVolume ? 1 : 0;
        }

        // 3. PrPoints (+2): any StrengthSet with IsPr = true that day
        var hasPr = await db.StrengthSets
            .AnyAsync(s => s.LiftSession.UserId == userId
                        && s.LiftSession.Date == date
                        && !s.LiftSession.IsDeleted
                        && s.IsPr, ct);
        var prPoints = hasPr ? 2 : 0;

        // 4. WodCompletionPoints (+2): DailyWodResult exists for the league's WOD today
        var todayWod = await db.DailyWods
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.LeagueId == leagueId && w.Date == date, ct);

        int wodCompletionPoints = 0;
        int wodRankingPoints = 0;

        if (todayWod != null)
        {
            var myResult = await db.DailyWodResults
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.DailyWodId == todayWod.Id && r.UserId == userId, ct);

            if (myResult != null)
            {
                wodCompletionPoints = 2;

                // 5. WodRankingPoints (+2): user has best score
                var allResults = await db.DailyWodResults
                    .AsNoTracking()
                    .Where(r => r.DailyWodId == todayWod.Id)
                    .ToListAsync(ct);

                var isBest = IsBestResult(todayWod.Type, myResult, allResults);
                wodRankingPoints = isBest ? 2 : 0;
            }
        }

        // 6. StreakPoints (+2): AttendancePoints > 0 in the 2 previous calendar days
        var prevDay1 = date.AddDays(-1);
        var prevDay2 = date.AddDays(-2);

        var hasDay1 = await db.DailyPoints
            .AnyAsync(p => p.UserId == userId && p.LeagueId == leagueId
                        && p.Date == prevDay1 && p.AttendancePoints > 0, ct);
        var hasDay2 = await db.DailyPoints
            .AnyAsync(p => p.UserId == userId && p.LeagueId == leagueId
                        && p.Date == prevDay2 && p.AttendancePoints > 0, ct);

        var streakPoints = (hasDay1 && hasDay2) ? 2 : 0;

        // Upsert DailyPoints
        var existing = await db.DailyPoints
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LeagueId == leagueId && p.Date == date, ct);

        if (existing == null)
        {
            db.DailyPoints.Add(new Domain.Entities.DailyPoints
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LeagueId = leagueId,
                Date = date,
                AttendancePoints = attendancePoints,
                VolumePoints = volumePoints,
                PrPoints = prPoints,
                WodCompletionPoints = wodCompletionPoints,
                WodRankingPoints = wodRankingPoints,
                StreakPoints = streakPoints,
                CalculatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.AttendancePoints = attendancePoints;
            existing.VolumePoints = volumePoints;
            existing.PrPoints = prPoints;
            existing.WodCompletionPoints = wodCompletionPoints;
            existing.WodRankingPoints = wodRankingPoints;
            existing.StreakPoints = streakPoints;
            existing.CalculatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }

    private static bool IsBestResult(
        string wodType,
        Domain.Entities.DailyWodResult myResult,
        List<Domain.Entities.DailyWodResult> allResults)
    {
        if (wodType == "ForTime")
        {
            // Lower elapsed seconds is better; exclude DNF
            var validResults = allResults
                .Where(r => !r.DidNotFinish && r.ElapsedSeconds.HasValue)
                .ToList();
            if (!myResult.ElapsedSeconds.HasValue || myResult.DidNotFinish)
                return false;
            var best = validResults
                .OrderBy(r => r.ElapsedSeconds)
                .ThenBy(r => r.CreatedAt)
                .FirstOrDefault();
            return best?.Id == myResult.Id;
        }
        else if (wodType == "AMRAP")
        {
            var validResults = allResults.Where(r => r.RoundsCompleted.HasValue).ToList();
            if (!myResult.RoundsCompleted.HasValue) return false;
            var best = validResults
                .OrderByDescending(r => r.RoundsCompleted)
                .ThenBy(r => r.CreatedAt)
                .FirstOrDefault();
            return best?.Id == myResult.Id;
        }
        else
        {
            // EMOM, Chipper, Intervals — total reps
            var validResults = allResults.Where(r => r.TotalReps.HasValue).ToList();
            if (!myResult.TotalReps.HasValue) return false;
            var best = validResults
                .OrderByDescending(r => r.TotalReps)
                .ThenBy(r => r.CreatedAt)
                .FirstOrDefault();
            return best?.Id == myResult.Id;
        }
    }
}
