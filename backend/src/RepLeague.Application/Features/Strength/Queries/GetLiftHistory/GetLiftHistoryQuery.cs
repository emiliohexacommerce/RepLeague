using MediatR;
using RepLeague.Application.Features.Strength.DTOs;

namespace RepLeague.Application.Features.Strength.Queries.GetLiftHistory;

public record GetLiftHistoryQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<List<LiftSessionDto>>;
