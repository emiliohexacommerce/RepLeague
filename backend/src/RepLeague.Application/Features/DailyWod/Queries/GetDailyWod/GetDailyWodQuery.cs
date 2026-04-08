using MediatR;
using RepLeague.Application.Features.DailyWod.DTOs;

namespace RepLeague.Application.Features.DailyWod.Queries.GetDailyWod;

public record GetDailyWodQuery(Guid LeagueId, Guid RequestingUserId) : IRequest<DailyWodDto?>;
