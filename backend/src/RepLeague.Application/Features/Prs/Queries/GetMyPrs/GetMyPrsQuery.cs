using MediatR;
using RepLeague.Application.Features.Prs.DTOs;

namespace RepLeague.Application.Features.Prs.Queries.GetMyPrs;

public record GetMyPrsQuery(Guid UserId) : IRequest<List<PrDto>>;
