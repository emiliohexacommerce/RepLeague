using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepLeague.Application.Features.DailyWod.Commands.ActivateLeaguePoints;
using RepLeague.Application.Features.DailyWod.Commands.SetDailyWod;
using RepLeague.Application.Features.DailyWod.Commands.SubmitWodResult;
using RepLeague.Application.Features.DailyWod.DTOs;
using RepLeague.Application.Features.DailyWod.Queries.GetDailyWod;
using RepLeague.Application.Features.DailyWod.Queries.GetDailyWodResults;

namespace RepLeague.API.Controllers;

[Authorize]
[Route("api/leagues/{leagueId:guid}/daily-wod")]
public class DailyWodController(IMediator mediator) : BaseApiController(mediator)
{
    /// <summary>Activate the points system for a league.</summary>
    [HttpPost("activate")]
    [ProducesResponseType(typeof(ActivateLeaguePointsResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate(
        Guid leagueId,
        [FromBody] ActivateLeaguePointsRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ActivateLeaguePointsCommand(leagueId, CurrentUserId, request.RunBackfill), ct);
        return Ok(result);
    }

    /// <summary>Get today's WOD for the league.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(DailyWodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetDailyWod(Guid leagueId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetDailyWodQuery(leagueId, CurrentUserId), ct);
        return result is null ? NoContent() : Ok(result);
    }

    /// <summary>Set today's WOD for the league (first athlete of the day).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(DailyWodDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> SetDailyWod(
        Guid leagueId,
        [FromBody] SetDailyWodRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new SetDailyWodCommand(
                leagueId,
                CurrentUserId,
                request.Type,
                request.Title,
                request.TimeCapSeconds,
                request.Notes,
                request.Exercises), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Submit a result for today's WOD.</summary>
    [HttpPost("results")]
    [ProducesResponseType(typeof(DailyWodResultDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> SubmitResult(
        Guid leagueId,
        [FromBody] SubmitWodResultRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new SubmitWodResultCommand(
                leagueId,
                CurrentUserId,
                request.ElapsedSeconds,
                request.RoundsCompleted,
                request.TotalReps,
                request.IsRx,
                request.DidNotFinish,
                request.Notes,
                request.ExerciseDetails), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Get all results for today's WOD.</summary>
    [HttpGet("results")]
    [ProducesResponseType(typeof(List<DailyWodResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResults(Guid leagueId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetDailyWodResultsQuery(leagueId, CurrentUserId), ct);
        return Ok(result);
    }
}

public record ActivateLeaguePointsRequest(bool RunBackfill);

public record SetDailyWodRequest(
    string Type,
    string Title,
    int? TimeCapSeconds,
    string? Notes,
    List<DailyWodExerciseRequest> Exercises
);

public record SubmitWodResultRequest(
    int? ElapsedSeconds,
    int? RoundsCompleted,
    int? TotalReps,
    bool IsRx,
    bool DidNotFinish,
    string? Notes,
    List<WodResultExerciseRequest> ExerciseDetails
);
