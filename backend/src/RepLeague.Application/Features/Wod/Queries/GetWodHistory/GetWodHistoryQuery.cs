using MediatR;
using RepLeague.Application.Features.Wod.DTOs;

namespace RepLeague.Application.Features.Wod.Queries.GetWodHistory;

public record GetWodHistoryQuery(
    Guid UserId,
    string? Type,
    int PageSize = 20,
    int Page = 1
) : IRequest<List<WodEntryDto>>;
