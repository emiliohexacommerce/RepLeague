using MediatR;
using RepLeague.Application.Features.DailyWod.DTOs;

namespace RepLeague.Application.Features.DailyWod.Commands.ActivateLeaguePoints;

public record ActivateLeaguePointsCommand(
    Guid LeagueId,
    Guid RequestingUserId,
    bool RunBackfill
) : IRequest<ActivateLeaguePointsResult>;
