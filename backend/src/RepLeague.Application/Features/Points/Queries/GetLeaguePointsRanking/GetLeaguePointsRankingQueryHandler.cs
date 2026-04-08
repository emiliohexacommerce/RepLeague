using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Points.DTOs;

namespace RepLeague.Application.Features.Points.Queries.GetLeaguePointsRanking;

public class GetLeaguePointsRankingQueryHandler(IAppDbContext db)
    : IRequestHandler<GetLeaguePointsRankingQuery, List<LeaguePointsRankingEntryDto>>
{
    public async Task<List<LeaguePointsRankingEntryDto>> Handle(
        GetLeaguePointsRankingQuery request, CancellationToken ct)
    {
        // Verify user is member of league
        var isMember = await db.LeagueMembers
            .AnyAsync(m => m.LeagueId == request.LeagueId && m.UserId == request.RequestingUserId, ct);
        if (!isMember)
            throw new UnauthorizedException("El usuario no es miembro de esta liga.");

        var (startDate, endDate) = GetDateRange(request.Period);

        var points = await db.DailyPoints
            .Include(p => p.User)
            .Where(p => p.LeagueId == request.LeagueId
                     && p.Date >= startDate
                     && p.Date <= endDate)
            .ToListAsync(ct);

        var grouped = points
            .GroupBy(p => p.UserId)
            .Select(g =>
            {
                var user = g.First().User;
                var isAnonymous = user.Visibility == "private";
                var totalPoints = g.Sum(p => p.AttendancePoints + p.VolumePoints + p.PrPoints
                                           + p.WodCompletionPoints + p.WodRankingPoints + p.StreakPoints);
                var trainingDays = g.Count(p => p.AttendancePoints > 0);

                return new
                {
                    UserId = g.Key,
                    User = user,
                    IsAnonymous = isAnonymous,
                    TotalPoints = totalPoints,
                    AttendancePoints = g.Sum(p => p.AttendancePoints),
                    VolumePoints = g.Sum(p => p.VolumePoints),
                    PrPoints = g.Sum(p => p.PrPoints),
                    WodCompletionPoints = g.Sum(p => p.WodCompletionPoints),
                    WodRankingPoints = g.Sum(p => p.WodRankingPoints),
                    StreakPoints = g.Sum(p => p.StreakPoints),
                    TrainingDays = trainingDays
                };
            })
            .OrderByDescending(x => x.TotalPoints)
            .ThenByDescending(x => x.TrainingDays)
            .ToList();

        return grouped
            .Select((x, index) => new LeaguePointsRankingEntryDto(
                Position: index + 1,
                UserId: x.UserId,
                DisplayName: x.IsAnonymous ? "Atleta Anónimo" : x.User.DisplayName,
                AvatarUrl: x.IsAnonymous ? null : x.User.AvatarUrl,
                Country: x.IsAnonymous ? null : x.User.Country,
                City: x.IsAnonymous ? null : x.User.City,
                GymName: x.IsAnonymous ? null : x.User.GymName,
                IsAnonymous: x.IsAnonymous,
                TotalPoints: x.TotalPoints,
                AttendancePoints: x.AttendancePoints,
                VolumePoints: x.VolumePoints,
                PrPoints: x.PrPoints,
                WodCompletionPoints: x.WodCompletionPoints,
                WodRankingPoints: x.WodRankingPoints,
                StreakPoints: x.StreakPoints,
                TrainingDays: x.TrainingDays
            ))
            .ToList();
    }

    private static (DateOnly startDate, DateOnly endDate) GetDateRange(string period)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return period.ToLowerInvariant() switch
        {
            "daily" => (today, today),
            "weekly" => GetWeekRange(today),
            "monthly" => GetMonthRange(today),
            _ => (today, today)
        };
    }

    private static (DateOnly, DateOnly) GetWeekRange(DateOnly today)
    {
        // Monday to Sunday
        var dayOfWeek = (int)today.DayOfWeek;
        var daysFromMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        var monday = today.AddDays(-daysFromMonday);
        var sunday = monday.AddDays(6);
        return (monday, sunday);
    }

    private static (DateOnly, DateOnly) GetMonthRange(DateOnly today)
    {
        var firstDay = new DateOnly(today.Year, today.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        return (firstDay, lastDay);
    }
}
