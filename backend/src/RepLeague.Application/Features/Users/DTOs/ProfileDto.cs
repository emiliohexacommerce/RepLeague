namespace RepLeague.Application.Features.Users.DTOs;

public record ProfileDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? Country,
    string? Bio,
    DateTime CreatedAt,
    UserStatsDto Stats,
    // Extended profile fields
    string? Phone,
    DateOnly? BirthDate,
    string? City,
    string? GymName,
    string Units,
    string OneRmMethod,
    string Visibility,
    bool MarketingConsent
);

public record UserStatsDto(
    int TotalWorkouts,
    int TotalPrs,
    int LeagueCount
);

// ── Profile Summary (overview tab) ───────────────────────────────────────────

public record ProfileSummaryDto(
    int StreakWeeks,
    int TotalWods,
    int TotalLiftSessions,
    List<LeagueSummaryDto> Leagues,
    List<PrSummaryDto> TopPrs,
    List<RecentWodDto> RecentWods
);

public record LeagueSummaryDto(
    Guid LeagueId,
    string LeagueName,
    int Points,
    int Rank,
    int MembersCount
);

public record PrSummaryDto(
    string ExerciseName,
    decimal BestWeightKg,
    decimal? Best1RmKg,
    DateOnly AchievedAt
);

public record RecentWodDto(
    Guid Id,
    string Type,
    string? Title,
    DateOnly Date,
    string? ElapsedTime,
    bool RxScaled
);

// ── Strength progress chart ────────────────────────────────────────────────

public record StrengthChartPointDto(DateOnly Date, decimal OneRepMaxKg);
