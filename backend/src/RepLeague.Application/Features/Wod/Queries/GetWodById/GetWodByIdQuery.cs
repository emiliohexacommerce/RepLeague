using MediatR;
using RepLeague.Application.Features.Wod.DTOs;

namespace RepLeague.Application.Features.Wod.Queries.GetWodById;

public record GetWodByIdQuery(Guid WodEntryId, Guid UserId) : IRequest<WodEntryDto>;
