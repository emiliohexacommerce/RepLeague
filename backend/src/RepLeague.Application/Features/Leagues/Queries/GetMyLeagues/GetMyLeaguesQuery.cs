using MediatR;
using RepLeague.Application.Features.Leagues.DTOs;

namespace RepLeague.Application.Features.Leagues.Queries.GetMyLeagues;

public record GetMyLeaguesQuery(Guid UserId) : IRequest<List<LeagueDto>>;
