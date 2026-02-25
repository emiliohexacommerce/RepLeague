using MediatR;
using RepLeague.Application.Features.Leagues.DTOs;

namespace RepLeague.Application.Features.Leagues.Queries.GetLeagueById;

public record GetLeagueByIdQuery(Guid LeagueId, Guid RequesterId) : IRequest<LeagueDto>;
