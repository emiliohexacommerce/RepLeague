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
