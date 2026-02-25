using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using RepLeague.Application.Features.Workouts.Commands.CreateWorkout;
using RepLeague.Application.Features.Workouts.DTOs;
using RepLeague.Application.Features.Workouts.Queries.GetWorkoutById;
using RepLeague.Application.Features.Workouts.Queries.GetWorkouts;
using RepLeague.Domain.Enums;

namespace RepLeague.API.Controllers;

[Authorize]
public class WorkoutsController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpPost]
    [ProducesResponseType(typeof(WorkoutDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWorkoutRequest request, CancellationToken ct)
    {
        var command = new CreateWorkoutCommand(
            CurrentUserId,
            request.Type,
            request.Notes,
            request.Exercises,
            request.Wod);

        var result = await Mediator.Send(command, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<WorkoutDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? range, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetWorkoutsQuery(CurrentUserId, range), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetWorkoutByIdQuery(id, CurrentUserId), ct);
        return Ok(result);
    }
}

// ── Request models (separate from commands to keep UserId server-side) ────────
public record CreateWorkoutRequest(
    WorkoutType Type,
    string? Notes,
    List<CreateExerciseDto>? Exercises,
    CreateWodDto? Wod
);
