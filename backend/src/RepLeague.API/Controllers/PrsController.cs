using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepLeague.Application.Features.Prs.DTOs;
using RepLeague.Application.Features.Prs.Queries.GetMyPrs;

namespace RepLeague.API.Controllers;

[Authorize]
public class PrsController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet]
    [ProducesResponseType(typeof(List<PrDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPrs(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMyPrsQuery(CurrentUserId), ct);
        return Ok(result);
    }
}
