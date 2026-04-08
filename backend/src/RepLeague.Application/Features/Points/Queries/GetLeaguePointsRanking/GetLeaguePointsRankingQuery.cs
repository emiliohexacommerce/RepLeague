using MediatR;
using RepLeague.Application.Features.Points.DTOs;

namespace RepLeague.Application.Features.Points.Queries.GetLeaguePointsRanking;

public record GetLeaguePointsRankingQuery(
    Guid LeagueId,
    string Period,
    Guid RequestingUserId
) : IRequest<List<LeaguePointsRankingEntryDto>>;
