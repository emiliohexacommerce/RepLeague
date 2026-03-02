using MediatR;
using RepLeague.Application.Features.Dashboard.DTOs;

namespace RepLeague.Application.Features.Dashboard.Queries.GetDashboardOverview;

public record GetDashboardOverviewQuery(Guid UserId) : IRequest<DashboardOverviewDto>;
