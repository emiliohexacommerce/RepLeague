using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Common.Utils;
using RepLeague.Application.Features.Dashboard.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Dashboard.Queries.GetDashboardOverview;

public class GetDashboardOverviewQueryHandler(IAppDbContext db)
    : IRequestHandler<GetDashboardOverviewQuery, DashboardOverviewDto>
{
    public async Task<DashboardOverviewDto> Handle(GetDashboardOverviewQuery request, CancellationToken ct)
    {
        var userId = request.UserId;
        var today         = DateOnly.FromDateTime(DateTime.UtcNow);
        var ninetyDaysAgo = today.AddDays(-90);
        var sevenDaysAgo  = today.AddDays(-7);
        var twentyEightDaysAgo = today.AddDays(-28);
        var eightyFourDaysAgo  = today.AddDays(-84); // 12 weeks

        // ── Query 1: user preferences ─────────────────────────────────────
        var user = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.DisplayName, u.AvatarUrl, u.Units, u.OneRmMethod })
            .FirstOrDefaultAsync(ct)
            ?? throw new UnauthorizedAccessException("User not found.");

        // ── Query 2: WOD entries (last 90 days) ───────────────────────────
        var wods = await db.WodEntries
            .AsNoTracking()
            .Include(w => w.AmrapResult)
            .Where(w => w.UserId == userId && !w.IsDeleted && w.Date >= ninetyDaysAgo)
            .OrderByDescending(w => w.Date)
            .ThenByDescending(w => w.CreatedAt)
            .ToListAsync(ct);

        // ── Query 3: lift sessions (last 90 days) with sets ───────────────
        var liftSessions = await db.LiftSessions
            .AsNoTracking()
            .Include(s => s.Sets)
            .Where(s => s.UserId == userId && s.Date >= ninetyDaysAgo)
            .OrderByDescending(s => s.Date)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        // ── Query 4: league memberships + rankings ────────────────────────
        var memberships = await db.LeagueMembers
            .AsNoTracking()
            .Include(m => m.League)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.JoinedAt)
            .ToListAsync(ct);

        var leagueIds = memberships.Select(m => m.LeagueId).ToList();
        var allRankings = leagueIds.Count > 0
            ? await db.RankingEntries
                .AsNoTracking()
                .Where(r => leagueIds.Contains(r.LeagueId))
                .ToListAsync(ct)
            : new List<RankingEntry>();

        // ── Welcome ───────────────────────────────────────────────────────
        var firstName = user.DisplayName.Split(' ')[0];
        var welcome = new WelcomeDto(firstName, user.DisplayName, user.AvatarUrl);

        // ── Streak ────────────────────────────────────────────────────────
        var streak = new StreakDto(
            ComputeStreak(liftSessions.Select(s => s.Date).Concat(wods.Select(w => w.Date))));

        // ── Weekly summary (last 7 days) ──────────────────────────────────
        var wodsWeek = wods.Where(w => w.Date >= sevenDaysAgo).ToList();
        var liftsWeek = liftSessions.Where(s => s.Date >= sevenDaysAgo).ToList();
        var setsWeek = liftsWeek.SelectMany(s => s.Sets.Where(x => !x.IsWarmup)).ToList();
        var prsWeek = setsWeek.Count(s => s.IsPr);
        var volumeWeek = Math.Round(setsWeek.Sum(s => s.WeightKg * s.Reps), 0);
        var timeWeek = wodsWeek.Sum(w => w.ElapsedSeconds ?? 0);
        var sessionsWeek = wodsWeek.Count + liftsWeek.Count;
        var weeklySummary = new WeeklySummaryDto(sessionsWeek, prsWeek, volumeWeek, timeWeek);

        // ── KPIs ──────────────────────────────────────────────────────────
        var bestForTime = wods
            .Where(w => w.Type == "ForTime" && w.ElapsedSeconds.HasValue)
            .MinBy(w => w.ElapsedSeconds);
        var kpis = new List<KpiDto>
        {
            new("prs_week",    "PRs semana",    prsWeek.ToString()),
            new("sessions_7d", "Sesiones 7d",   sessionsWeek.ToString()),
            new("volume_week", "Volumen semana", $"{volumeWeek} kg"),
            new("best_fortime","Mejor ForTime",
                TimeParser.FormatSeconds(bestForTime?.ElapsedSeconds) ?? "—"),
        };

        // ── Recent WODs (top 5) ───────────────────────────────────────────
        var recentWods = wods.Take(5).Select(w => new RecentWodDashDto(
            w.Id, w.Type, w.Title, w.Date,
            TimeParser.FormatSeconds(w.ElapsedSeconds),
            w.RxScaled,
            w.AmrapResult?.RoundsCompleted,
            w.AmrapResult?.ExtraReps
        )).ToList();

        // ── Recent lifts (top 5 sessions → heaviest non-warmup set) ───────
        var recentLifts = liftSessions.Take(5).Select(s =>
        {
            var best = s.Sets
                .Where(x => !x.IsWarmup)
                .OrderByDescending(x => x.WeightKg)
                .FirstOrDefault()
                ?? s.Sets.FirstOrDefault();
            return best is null ? null : new RecentLiftDashDto(
                s.Id, best.ExerciseName, best.Reps, best.WeightKg,
                best.OneRepMaxKg, best.IsPr, s.Date);
        }).Where(x => x != null).Select(x => x!).ToList();

        // ── Leagues (top 3) ───────────────────────────────────────────────
        var leagues = memberships.Take(3).Select(m =>
        {
            var ranked = allRankings
                .Where(r => r.LeagueId == m.LeagueId)
                .OrderByDescending(r => r.Points)
                .ThenByDescending(r => r.PrCount)
                .ToList();
            var rank = ranked.FindIndex(r => r.UserId == userId) + 1;
            var mine = ranked.FirstOrDefault(r => r.UserId == userId);
            return new LeagueDashDto(
                m.LeagueId, m.League.Name,
                mine?.Points ?? 0,
                rank > 0 ? rank : ranked.Count + 1,
                ranked.Count);
        }).ToList();

        // ── Charts ────────────────────────────────────────────────────────
        // Determine top exercise (overall highest 1RM)
        string? topExercise = null;
        decimal topRm = 0;
        foreach (var session in liftSessions)
            foreach (var set in session.Sets.Where(s => !s.IsWarmup && s.OneRepMaxKg.HasValue))
                if (set.OneRepMaxKg!.Value > topRm) { topRm = set.OneRepMaxKg.Value; topExercise = set.ExerciseName; }

        // 1RM sparkline (top exercise per ISO week)
        var rm1Points = new SortedDictionary<int, decimal>();
        // Volume (12w per ISO week)
        var volPoints = new SortedDictionary<int, decimal>();

        foreach (var session in liftSessions)
        {
            var wk = WeekKey(session.Date);
            foreach (var set in session.Sets.Where(s => !s.IsWarmup))
            {
                if (set.ExerciseName == topExercise && set.OneRepMaxKg.HasValue)
                    if (!rm1Points.TryGetValue(wk, out var prev) || set.OneRepMaxKg.Value > prev)
                        rm1Points[wk] = Math.Round(set.OneRepMaxKg.Value, 1);

                if (session.Date >= eightyFourDaysAgo)
                    volPoints[wk] = Math.Round(
                        volPoints.GetValueOrDefault(wk) + set.WeightKg * set.Reps, 0);
            }
        }

        // ForTime best per ISO week
        var ftPoints = new SortedDictionary<int, int>();
        foreach (var w in wods.Where(x => x.Type == "ForTime" && x.ElapsedSeconds.HasValue))
        {
            var wk = WeekKey(w.Date);
            if (!ftPoints.TryGetValue(wk, out var prev) || w.ElapsedSeconds!.Value < prev)
                ftPoints[wk] = w.ElapsedSeconds!.Value;
        }

        var charts = new DashboardChartsDto(
            new ChartDatasetDto(rm1Points.Keys.Select(WeekLabel).ToList(), rm1Points.Values.ToList()),
            new ChartDatasetDto(volPoints.Keys.Select(WeekLabel).ToList(), volPoints.Values.ToList()),
            new ChartDatasetDto(ftPoints.Keys.Select(WeekLabel).ToList(),
                ftPoints.Values.Select(v => (decimal)v).ToList()),
            topExercise);

        // ── Recommendations ───────────────────────────────────────────────
        var recommendations = BuildRecommendations(
            wods, liftSessions, topExercise, twentyEightDaysAgo, today);

        return new DashboardOverviewDto(
            welcome, streak, weeklySummary, kpis,
            recentWods, recentLifts, leagues,
            charts, recommendations);
    }

    // ── Recommendations engine (rule-based) ───────────────────────────────
    private static List<RecommendationDto> BuildRecommendations(
        List<WodEntry> wods,
        List<LiftSession> liftSessions,
        string? topExercise,
        DateOnly fourWeeksAgo,
        DateOnly today)
    {
        var result = new List<RecommendationDto>();

        var lastDate = liftSessions.Select(s => s.Date)
            .Concat(wods.Select(w => w.Date))
            .Order()
            .LastOrDefault();

        var daysSinceLast = lastDate == default ? 999 : today.DayNumber - lastDate.DayNumber;

        // Rule 1 — inactivity ≥ 4 days
        if (daysSinceLast >= 4)
            result.Add(new RecommendationDto(
                "low-activity",
                "Vuelve con un AMRAP 12'",
                $"Llevas {daysSinceLast} día(s) sin sesión. Prueba 10 burpees + 15 KB swings + 20 box jumps, AMRAP 12'.",
                new CtaDto("Registrar WOD", "/wod")));

        // Rule 2 — plateau en ejercicio favorito (sin PR en 4 semanas)
        if (topExercise != null && result.Count < 3)
        {
            var hasRecentPr = liftSessions
                .Where(s => s.Date >= fourWeeksAgo)
                .SelectMany(s => s.Sets)
                .Any(s => s.ExerciseName == topExercise && s.IsPr);

            if (!hasRecentPr)
                result.Add(new RecommendationDto(
                    "plateau",
                    $"Rompe el estancamiento en {topExercise}",
                    $"Sin PR en {topExercise} hace más de 4 semanas. Intenta 5×3 al 85–90% y varía la velocidad de ejecución.",
                    new CtaDto("Ver levantamientos", "/strength")));
        }

        // Rule 3 — baja consistencia (< 2 sesiones/semana promedio en 4 semanas)
        if (result.Count < 3)
        {
            var recentCount = liftSessions.Count(s => s.Date >= fourWeeksAgo)
                            + wods.Count(w => w.Date >= fourWeeksAgo);
            var avg = recentCount / 4.0;
            if (avg < 2.0)
                result.Add(new RecommendationDto(
                    "consistency",
                    "Mejora tu consistencia",
                    $"Promedio de {avg:F1} sesiones/semana en las últimas 4 semanas. Apunta a mínimo 2 sesiones fijas.",
                    new CtaDto("Mis ligas", "/leagues")));
        }

        return result;
    }

    // ── Streak helpers ────────────────────────────────────────────────────
    private static int ComputeStreak(IEnumerable<DateOnly> dates)
    {
        var weeks = dates
            .Select(d => ISOWeek.GetYear(d.ToDateTime(TimeOnly.MinValue)) * 100
                       + ISOWeek.GetWeekOfYear(d.ToDateTime(TimeOnly.MinValue)))
            .Distinct()
            .OrderDescending()
            .ToList();

        if (weeks.Count == 0) return 0;

        var now = DateTime.UtcNow;
        var curKey = ISOWeek.GetYear(now) * 100 + ISOWeek.GetWeekOfYear(now);
        if (weeks[0] < WeekKeyMinus(curKey)) return 0;

        int streak = 1;
        for (int i = 1; i < weeks.Count; i++)
        {
            if (weeks[i] == WeekKeyMinus(weeks[i - 1])) streak++;
            else break;
        }
        return streak;
    }

    private static int WeekKey(DateOnly d) =>
        ISOWeek.GetYear(d.ToDateTime(TimeOnly.MinValue)) * 100
        + ISOWeek.GetWeekOfYear(d.ToDateTime(TimeOnly.MinValue));

    private static string WeekLabel(int k) => $"{k / 100}-W{k % 100:D2}";

    private static int WeekKeyMinus(int k)
    {
        var y = k / 100; var w = k % 100;
        if (w == 1) { var py = y - 1; return py * 100 + ISOWeek.GetWeeksInYear(py); }
        return y * 100 + (w - 1);
    }
}
