using MediatR;
using RepLeague.Application.Features.Workouts.DTOs;

namespace RepLeague.Application.Features.Workouts.Queries.GetWorkouts;

public record GetWorkoutsQuery(Guid UserId, string? Range) : IRequest<List<WorkoutDto>>;
