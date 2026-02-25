using MediatR;
using RepLeague.Application.Features.Workouts.DTOs;

namespace RepLeague.Application.Features.Workouts.Queries.GetWorkoutById;

public record GetWorkoutByIdQuery(Guid WorkoutId, Guid UserId) : IRequest<WorkoutDto>;
