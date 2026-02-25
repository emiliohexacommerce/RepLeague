using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Workouts.DTOs;

namespace RepLeague.Application.Features.Workouts.Queries.GetWorkouts;

public class GetWorkoutsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetWorkoutsQuery, List<WorkoutDto>>
{
    public async Task<List<WorkoutDto>> Handle(GetWorkoutsQuery request, CancellationToken ct)
    {
        var cutoff = request.Range switch
        {
            "last7"  => DateTime.UtcNow.AddDays(-7),
            "last30" => DateTime.UtcNow.AddDays(-30),
            "last90" => DateTime.UtcNow.AddDays(-90),
            _        => DateTime.UtcNow.AddDays(-30)  // default last 30
        };

        var workouts = await db.Workouts
            .Include(w => w.Exercises)
            .Include(w => w.Wod)
            .Where(w => w.UserId == request.UserId && w.CreatedAt >= cutoff)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(ct);

        return workouts.Select(w => new WorkoutDto(
            w.Id,
            w.Type,
            w.IsPR,
            w.Notes,
            w.CreatedAt,
            w.Exercises.Select(e => new ExerciseDto(e.Id, e.ExerciseName, e.Sets, e.Reps, e.WeightKg)).ToList(),
            w.Wod == null ? null : new WodDto(
                w.Wod.WodName,
                w.Wod.Duration?.ToString(@"hh\:mm\:ss"),
                w.Wod.Rounds,
                w.Wod.TotalReps)
        )).ToList();
    }
}
