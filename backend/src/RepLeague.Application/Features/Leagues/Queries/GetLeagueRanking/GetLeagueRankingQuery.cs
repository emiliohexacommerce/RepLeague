using MediatR;
using RepLeague.Application.Features.Leagues.DTOs;

namespace RepLeague.Application.Features.Leagues.Queries.GetLeagueRanking;

public record GetLeagueRankingQuery(Guid LeagueId) : IRequest<List<LeagueRankingEntryDto>>;
