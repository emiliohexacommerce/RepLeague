using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepLeague.Application.Features.Dashboard.DTOs;
using RepLeague.Application.Features.Dashboard.Queries.GetDashboardOverview;

namespace RepLeague.API.Controllers;

[Authorize]
public class DashboardController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("/api/dashboard/overview")]
    [ProducesResponseType(typeof(DashboardOverviewDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetDashboardOverviewQuery(CurrentUserId), ct);
        return Ok(result);
    }
}
