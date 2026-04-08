namespace RepLeague.Application.Features.Leagues.DTOs;

public record LeagueDto(
    Guid Id,
    Guid OwnerUserId,
    string Name,
    string? Description,
    string? ImageUrl,
    int MemberCount,
    bool IsOwner,
    DateTime CreatedAt
);

public record LeagueMemberDto(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    DateTime JoinedAt
);

public record LeagueRankingEntryDto(
    int Rank,
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    int Points,
    int WorkoutCount,
    int PrCount
);

public record InvitationResultDto(
    Guid InvitationId,
    string JoinUrl,
    string Token
);

public record LiftSessionSummaryDto(
    Guid Id,
    DateOnly Date,
    string? Title,
    int SetCount
);

public record DailyWodResultSummaryDto(
    Guid Id,
    Guid DailyWodId,
    string WodTitle,
    string WodType,
    DateOnly Date,
    int? ElapsedSeconds,
    int? RoundsCompleted,
    int? TotalReps,
    bool IsRx,
    bool DidNotFinish
);

public record LeagueMemberProfileDto(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    string? Country,
    string? City,
    string? GymName,
    string? Bio,
    bool IsAnonymous,
    // Stats
    int TotalPointsThisMonth,
    int TrainingDaysThisMonth,
    int CurrentStreak,
    // Sessions (null si private)
    List<LiftSessionSummaryDto>? RecentLiftSessions,
    List<DailyWodResultSummaryDto>? RecentWodResults
);
