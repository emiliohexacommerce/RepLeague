namespace RepLeague.Application.Features.Dashboard.DTOs;

public record DashboardOverviewDto(
    WelcomeDto Welcome,
    StreakDto Streak,
    WeeklySummaryDto WeeklySummary,
    List<KpiDto> Kpis,
    List<RecentWodDashDto> RecentWods,
    List<RecentLiftDashDto> RecentLifts,
    List<LeagueDashDto> Leagues,
    DashboardChartsDto Charts,
    List<RecommendationDto> Recommendations
);

public record WelcomeDto(string FirstName, string DisplayName, string? AvatarUrl);

public record StreakDto(int Weeks);

public record WeeklySummaryDto(int Sessions, int Prs, decimal VolumeKg, int TimeSec);

public record KpiDto(string Key, string Label, string Value);

public record RecentWodDashDto(
    Guid Id,
    string Type,
    string? Title,
    DateOnly Date,
    string? ElapsedTime,
    bool RxScaled,
    int? Rounds,
    int? ExtraReps
);

public record RecentLiftDashDto(
    Guid SessionId,
    string Exercise,
    int Reps,
    decimal Weight,
    decimal? Est1Rm,
    bool IsPr,
    DateOnly Date
);

public record LeagueDashDto(
    Guid LeagueId,
    string Name,
    int Points,
    int Rank,
    int Members
);

public record DashboardChartsDto(
    ChartDatasetDto Strength1Rm,
    ChartDatasetDto WeeklyVolume,
    ChartDatasetDto ForTimeBest,
    string? TopExercise
);

public record ChartDatasetDto(List<string> Labels, List<decimal> Data);

public record RecommendationDto(
    string Code,
    string Title,
    string Body,
    CtaDto Cta
);

public record CtaDto(string Label, string Url);
