using MediatR;
using RepLeague.Application.Features.DailyWod.DTOs;

namespace RepLeague.Application.Features.DailyWod.Queries.GetDailyWodResults;

public record GetDailyWodResultsQuery(Guid LeagueId, Guid RequestingUserId) : IRequest<List<DailyWodResultDto>>;
