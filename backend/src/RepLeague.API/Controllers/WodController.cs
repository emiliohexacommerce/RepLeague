using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepLeague.Application.Features.Wod.Commands.CreateWodEntry;
using RepLeague.Application.Features.Wod.Commands.DeleteWodEntry;
using RepLeague.Application.Features.Wod.Queries.GetWodById;
using RepLeague.Application.Features.Wod.Queries.GetWodHistory;

namespace RepLeague.API.Controllers;

[Authorize]
public class WodController(IMediator mediator) : BaseApiController(mediator)
{
    /// <summary>Log a new WOD entry.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWodEntryCommand request, CancellationToken ct)
    {
        var command = request with { UserId = CurrentUserId };
        var result = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Get a single WOD entry by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetWodByIdQuery(id, CurrentUserId), ct);
        return Ok(result);
    }

    /// <summary>Get WOD history (paginated, filterable by type).</summary>
    [HttpGet]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetWodHistoryQuery(CurrentUserId, type, pageSize, page), ct);
        return Ok(result);
    }

    /// <summary>Soft-delete a WOD entry.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteWodEntryCommand(id, CurrentUserId), ct);
        return NoContent();
    }
}
