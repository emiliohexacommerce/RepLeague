namespace RepLeague.Application.Features.Users.DTOs;

public record ProfileDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? Country,
    string? Bio,
    DateTime CreatedAt,
    UserStatsDto Stats
);

public record UserStatsDto(
    int TotalWorkouts,
    int TotalPrs,
    int LeagueCount
);
