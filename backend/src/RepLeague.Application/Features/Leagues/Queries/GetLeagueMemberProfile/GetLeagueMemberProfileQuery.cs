using MediatR;
using RepLeague.Application.Features.Leagues.DTOs;

namespace RepLeague.Application.Features.Leagues.Queries.GetLeagueMemberProfile;

public record GetLeagueMemberProfileQuery(
    Guid LeagueId,
    Guid TargetUserId,
    Guid RequestingUserId
) : IRequest<LeagueMemberProfileDto>;
