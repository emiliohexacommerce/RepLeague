using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Leagues.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Leagues.Queries.GetLeagueMemberProfile;

public class GetLeagueMemberProfileQueryHandler(IAppDbContext db)
    : IRequestHandler<GetLeagueMemberProfileQuery, LeagueMemberProfileDto>
{
    public async Task<LeagueMemberProfileDto> Handle(
        GetLeagueMemberProfileQuery request, CancellationToken ct)
    {
        // Verify requesting user is member of league
        var isRequesterMember = await db.LeagueMembers
            .AnyAsync(m => m.LeagueId == request.LeagueId && m.UserId == request.RequestingUserId, ct);
        if (!isRequesterMember)
            throw new UnauthorizedException("El usuario no es miembro de esta liga.");

        // Verify target user is member of league
        var isTargetMember = await db.LeagueMembers
            .AnyAsync(m => m.LeagueId == request.LeagueId && m.UserId == request.TargetUserId, ct);
        if (!isTargetMember)
            throw new NotFoundException("League member", request.TargetUserId);

        var targetUser = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.TargetUserId, ct)
            ?? throw new NotFoundException(nameof(User), request.TargetUserId);

        var isAnonymous = targetUser.Visibility == "private";

        // Calculate stats for this month
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var firstOfMonth = new DateOnly(today.Year, today.Month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        var monthlyPoints = await db.DailyPoints
            .Where(p => p.UserId == request.TargetUserId
                     && p.LeagueId == request.LeagueId
                     && p.Date >= firstOfMonth
                     && p.Date <= lastOfMonth)
            .ToListAsync(ct);

        var totalPointsThisMonth = monthlyPoints
            .Sum(p => p.AttendancePoints + p.VolumePoints + p.PrPoints
                    + p.WodCompletionPoints + p.WodRankingPoints + p.StreakPoints);

        var trainingDaysThisMonth = monthlyPoints.Count(p => p.AttendancePoints > 0);

        // Calculate current streak
        var currentStreak = await CalculateCurrentStreakAsync(request.TargetUserId, request.LeagueId, today, ct);

        if (isAnonymous)
        {
            return new LeagueMemberProfileDto(
                UserId: request.TargetUserId,
                DisplayName: "Atleta Anónimo",
                AvatarUrl: null,
                Country: null,
                City: null,
                GymName: null,
                Bio: null,
                IsAnonymous: true,
                TotalPointsThisMonth: totalPointsThisMonth,
                TrainingDaysThisMonth: trainingDaysThisMonth,
                CurrentStreak: currentStreak,
                RecentLiftSessions: null,
                RecentWodResults: null
            );
        }

        // Full profile: last 10 LiftSessions + last 10 DailyWodResults
        var recentLiftSessions = await db.LiftSessions
            .Where(s => s.UserId == request.TargetUserId && !s.IsDeleted)
            .OrderByDescending(s => s.Date)
            .Take(10)
            .Select(s => new LiftSessionSummaryDto(
                s.Id,
                s.Date,
                s.Title,
                s.Sets.Count))
            .ToListAsync(ct);

        var recentWodResults = await db.DailyWodResults
            .Include(r => r.DailyWod)
            .Where(r => r.UserId == request.TargetUserId && r.DailyWod.LeagueId == request.LeagueId)
            .OrderByDescending(r => r.DailyWod.Date)
            .Take(10)
            .Select(r => new DailyWodResultSummaryDto(
                r.Id,
                r.DailyWodId,
                r.DailyWod.Title,
                r.DailyWod.Type,
                r.DailyWod.Date,
                r.ElapsedSeconds,
                r.RoundsCompleted,
                r.TotalReps,
                r.IsRx,
                r.DidNotFinish))
            .ToListAsync(ct);

        return new LeagueMemberProfileDto(
            UserId: request.TargetUserId,
            DisplayName: targetUser.DisplayName,
            AvatarUrl: targetUser.AvatarUrl,
            Country: targetUser.Country,
            City: targetUser.City,
            GymName: targetUser.GymName,
            Bio: targetUser.Bio,
            IsAnonymous: false,
            TotalPointsThisMonth: totalPointsThisMonth,
            TrainingDaysThisMonth: trainingDaysThisMonth,
            CurrentStreak: currentStreak,
            RecentLiftSessions: recentLiftSessions,
            RecentWodResults: recentWodResults
        );
    }

    private async Task<int> CalculateCurrentStreakAsync(
        Guid userId, Guid leagueId, DateOnly today, CancellationToken ct)
    {
        var recentPoints = await db.DailyPoints
            .Where(p => p.UserId == userId && p.LeagueId == leagueId && p.AttendancePoints > 0)
            .OrderByDescending(p => p.Date)
            .Take(365)
            .Select(p => p.Date)
            .ToListAsync(ct);

        if (recentPoints.Count == 0) return 0;

        var streak = 0;
        var checkDate = today;

        foreach (var date in recentPoints)
        {
            if (date == checkDate)
            {
                streak++;
                checkDate = checkDate.AddDays(-1);
            }
            else if (date < checkDate)
            {
                break;
            }
        }

        return streak;
    }
}
