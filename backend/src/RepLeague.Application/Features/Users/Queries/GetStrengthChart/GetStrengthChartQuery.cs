using MediatR;
using RepLeague.Application.Features.Users.DTOs;

namespace RepLeague.Application.Features.Users.Queries.GetStrengthChart;

public record GetStrengthChartQuery(Guid UserId, string Exercise) : IRequest<List<StrengthChartPointDto>>;
