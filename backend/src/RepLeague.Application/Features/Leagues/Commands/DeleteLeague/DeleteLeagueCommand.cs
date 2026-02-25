using MediatR;

namespace RepLeague.Application.Features.Leagues.Commands.DeleteLeague;

public record DeleteLeagueCommand(Guid LeagueId, Guid RequesterId) : IRequest;
