using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Workouts.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Workouts.Queries.GetWorkoutById;

public class GetWorkoutByIdQueryHandler(IAppDbContext db)
    : IRequestHandler<GetWorkoutByIdQuery, WorkoutDto>
{
    public async Task<WorkoutDto> Handle(GetWorkoutByIdQuery request, CancellationToken ct)
    {
        var workout = await db.Workouts
            .Include(w => w.Exercises)
            .Include(w => w.Wod)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId, ct)
            ?? throw new NotFoundException(nameof(Workout), request.WorkoutId);

        // Users can only see their own workouts
        if (workout.UserId != request.UserId)
            throw new NotFoundException(nameof(Workout), request.WorkoutId);

        return new WorkoutDto(
            workout.Id,
            workout.Type,
            workout.IsPR,
            workout.Notes,
            workout.CreatedAt,
            workout.Exercises.Select(e => new ExerciseDto(e.Id, e.ExerciseName, e.Sets, e.Reps, e.WeightKg)).ToList(),
            workout.Wod == null ? null : new WodDto(
                workout.Wod.WodName,
                workout.Wod.Duration?.ToString(@"hh\:mm\:ss"),
                workout.Wod.Rounds,
                workout.Wod.TotalReps)
        );
    }
}
