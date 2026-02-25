using MediatR;
using RepLeague.Application.Features.Leagues.DTOs;

namespace RepLeague.Application.Features.Leagues.Commands.CreateLeague;

public record CreateLeagueCommand(
    Guid OwnerId,
    string Name,
    string? Description
) : IRequest<LeagueDto>;
