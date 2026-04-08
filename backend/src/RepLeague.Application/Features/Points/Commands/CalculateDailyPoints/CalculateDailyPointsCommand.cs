using MediatR;

namespace RepLeague.Application.Features.Points.Commands.CalculateDailyPoints;

public record CalculateDailyPointsCommand(
    Guid UserId,
    Guid LeagueId,
    DateOnly Date
) : IRequest<Unit>;
