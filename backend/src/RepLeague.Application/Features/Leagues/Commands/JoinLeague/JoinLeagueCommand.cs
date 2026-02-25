using MediatR;
using RepLeague.Application.Features.Leagues.DTOs;

namespace RepLeague.Application.Features.Leagues.Commands.JoinLeague;

public record JoinLeagueCommand(
    string Token,
    Guid UserId
) : IRequest<LeagueDto>;
