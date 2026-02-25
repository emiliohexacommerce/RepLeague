using MediatR;
using RepLeague.Application.Features.Workouts.DTOs;
using RepLeague.Domain.Enums;

namespace RepLeague.Application.Features.Workouts.Commands.CreateWorkout;

public record CreateWorkoutCommand(
    Guid UserId,
    WorkoutType Type,
    string? Notes,
    List<CreateExerciseDto>? Exercises,
    CreateWodDto? Wod
) : IRequest<WorkoutDto>;
