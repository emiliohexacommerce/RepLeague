using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepLeague.Application.Features.Strength.Commands.AddManualLiftPr;
using RepLeague.Application.Features.Strength.Commands.CreateLiftSession;
using RepLeague.Application.Features.Strength.Commands.DeleteLiftSession;
using RepLeague.Application.Features.Strength.Queries.GetLiftHistory;
using RepLeague.Application.Features.Strength.Queries.GetLiftPrs;
using RepLeague.Application.Features.Strength.Queries.GetManualLiftPrs;

namespace RepLeague.API.Controllers;

[Authorize]
public class StrengthController(IMediator mediator) : BaseApiController(mediator)
{
    /// <summary>Log a new lift session.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLiftSessionCommand request, CancellationToken ct)
    {
        var command = request with { UserId = CurrentUserId };
        var result = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetHistory), new { }, result);
    }

    /// <summary>Get lift history (paginated).</summary>
    [HttpGet]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetLiftHistoryQuery(CurrentUserId, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Get personal records per exercise (best 1RM).</summary>
    [HttpGet("prs")]
    public async Task<IActionResult> GetPrs(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetLiftPrsQuery(CurrentUserId), ct);
        return Ok(result);
    }

    /// <summary>Soft-delete a lift session.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteLiftSessionCommand(id, CurrentUserId), ct);
        return NoContent();
    }

    // ── Manual PRs ──────────────────────────────────────────────────────────

    /// <summary>Get all manually-entered PRs grouped by exercise (with history).</summary>
    [HttpGet("manual-prs")]
    public async Task<IActionResult> GetManualPrs(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetManualLiftPrsQuery(CurrentUserId), ct);
        return Ok(result);
    }

    /// <summary>Add a new manual PR entry for an exercise.</summary>
    [HttpPost("manual-prs")]
    public async Task<IActionResult> AddManualPr([FromBody] AddManualLiftPrRequest request, CancellationToken ct)
    {
        var command = new AddManualLiftPrCommand(
            CurrentUserId,
            request.ExerciseName,
            request.WeightKg,
            request.Notes,
            request.AchievedAt);

        var result = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetManualPrs), result);
    }
}

public record AddManualLiftPrRequest(
    string ExerciseName,
    decimal WeightKg,
    string? Notes,
    DateOnly AchievedAt
);
