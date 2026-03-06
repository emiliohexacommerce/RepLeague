using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Common.Utils;
using RepLeague.Application.Features.Users.DTOs;

namespace RepLeague.Application.Features.Users.Queries.GetProfileSummary;

public class GetProfileSummaryQueryHandler(IAppDbContext db)
    : IRequestHandler<GetProfileSummaryQuery, ProfileSummaryDto>
{
    public async Task<ProfileSummaryDto> Handle(GetProfileSummaryQuery request, CancellationToken ct)
    {
        var userId = request.UserId;

        // ── Leagues with rank ─────────────────────────────────────────────
        var memberships = await db.LeagueMembers
            .Include(m => m.League)
            .Where(m => m.UserId == userId)
            .ToListAsync(ct);

        var leagueIds = memberships.Select(m => m.LeagueId).ToList();

        var allRankings = await db.RankingEntries
            .Where(r => leagueIds.Contains(r.LeagueId))
            .ToListAsync(ct);

        var leagues = memberships.Select(m =>
        {
            var ranked = allRankings
                .Where(r => r.LeagueId == m.LeagueId)
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.PrCount)
                .ToList();

            var rank = ranked.FindIndex(r => r.UserId == userId) + 1;
            var myEntry = ranked.FirstOrDefault(r => r.UserId == userId);

            return new LeagueSummaryDto(
                m.LeagueId,
                m.League.Name,
                myEntry?.Points ?? 0,
                rank > 0 ? rank : ranked.Count + 1,
                ranked.Count
            );
        }).ToList();

        // ── Top PRs (from ManualLiftPrs) ────────────────────────────────────
        var manualPrs = await db.ManualLiftPrs
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .ToListAsync(ct);

        var topPrs = manualPrs
            .GroupBy(x => x.ExerciseName)
            .Select(g =>
            {
                var best = g.MaxBy(x => x.WeightKg)!;
                return new PrSummaryDto(
                    g.Key,
                    best.WeightKg,
                    best.AchievedAt
                );
            })
            .OrderByDescending(p => p.BestWeightKg)
            .Take(8)
            .ToList();

        // ── Recent WODs ───────────────────────────────────────────────────
        var recentWods = await db.WodEntries
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.Date)
            .ThenByDescending(w => w.CreatedAt)
            .Take(5)
            .ToListAsync(ct);

        var recentWodDtos = recentWods.Select(w => new RecentWodDto(
            w.Id,
            w.Type,
            w.Title,
            w.Date,
            TimeParser.FormatSeconds(w.ElapsedSeconds),
            w.RxScaled
        )).ToList();

        // ── Totals ────────────────────────────────────────────────────────
        var totalWods = await db.WodEntries.CountAsync(w => w.UserId == userId, ct);
        var totalLiftSessions = await db.LiftSessions.CountAsync(s => s.UserId == userId, ct);

        // ── Streak (consecutive calendar weeks with ≥1 session) ──────────
        var liftDates = await db.LiftSessions
            .Where(s => s.UserId == userId)
            .Select(s => s.Date)
            .ToListAsync(ct);

        var wodDates = await db.WodEntries
            .Where(w => w.UserId == userId)
            .Select(w => w.Date)
            .ToListAsync(ct);

        var streak = ComputeStreak(liftDates.Concat(wodDates));

        return new ProfileSummaryDto(streak, totalWods, totalLiftSessions, leagues, topPrs, recentWodDtos);
    }

    private static int ComputeStreak(IEnumerable<DateOnly> activityDates)
    {
        var weeks = activityDates
            .Select(d => ISOWeek.GetYear(d.ToDateTime(TimeOnly.MinValue)) * 100
                       + ISOWeek.GetWeekOfYear(d.ToDateTime(TimeOnly.MinValue)))
            .Distinct()
            .OrderByDescending(w => w)
            .ToList();

        if (weeks.Count == 0) return 0;

        // Current ISO week key
        var now = DateTime.UtcNow;
        var currentWeekKey = ISOWeek.GetYear(now) * 100 + ISOWeek.GetWeekOfYear(now);
        var previousWeekKey = WeekKeyMinus(currentWeekKey);

        // Accept streak starting from this week or last week
        var startKey = weeks[0] >= previousWeekKey ? weeks[0] : -1;
        if (startKey == -1) return 0;

        int streak = 1;
        for (int i = 1; i < weeks.Count; i++)
        {
            if (weeks[i] == WeekKeyMinus(weeks[i - 1]))
                streak++;
            else
                break;
        }

        return streak;
    }

    private static int WeekKeyMinus(int weekKey)
    {
        var year = weekKey / 100;
        var week = weekKey % 100;
        if (week == 1)
        {
            var prevYear = year - 1;
            return prevYear * 100 + ISOWeek.GetWeeksInYear(prevYear);
        }
        return year * 100 + (week - 1);
    }
}
