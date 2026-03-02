using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Prs.DTOs;

namespace RepLeague.Application.Features.Prs.Queries.GetMyPrs;

public class GetMyPrsQueryHandler(IAppDbContext db) : IRequestHandler<GetMyPrsQuery, List<PrDto>>
{
    public async Task<List<PrDto>> Handle(GetMyPrsQuery request, CancellationToken ct)
    {
        // Load all exercises for the user's workouts
        var exercises = await db.WorkoutExercises
            .Where(e => e.Workout.UserId == request.UserId)
            .Select(e => new
            {
                e.ExerciseName,
                e.WeightKg,
                e.Sets,
                e.Reps,
                AchievedAt = e.Workout.CreatedAt
            })
            .ToListAsync(ct);

        var strengthPrs = exercises
            .GroupBy(e => e.ExerciseName)
            .Select(g =>
            {
                var best = g.OrderByDescending(e => e.WeightKg).ThenByDescending(e => e.AchievedAt).First();
                return new PrDto(best.ExerciseName, "Strength", best.WeightKg, null, best.Sets, best.Reps, best.AchievedAt);
            })
            .OrderBy(p => p.Name)
            .ToList();

        // Load all WODs for the user's workouts
        var wods = await db.WorkoutWods
            .Where(w => w.Workout.UserId == request.UserId && w.Duration != null)
            .Select(w => new
            {
                w.WodName,
                w.Duration,
                w.Rounds,
                AchievedAt = w.Workout.CreatedAt
            })
            .ToListAsync(ct);

        var wodPrs = wods
            .GroupBy(w => w.WodName)
            .Select(g =>
            {
                var best = g.OrderBy(w => w.Duration).ThenByDescending(w => w.AchievedAt).First();
                return new PrDto(
                    best.WodName,
                    "WOD",
                    null,
                    best.Duration?.ToString(@"hh\:mm\:ss"),
                    null,
                    null,
                    best.AchievedAt);
            })
            .OrderBy(p => p.Name)
            .ToList();

        return [.. strengthPrs, .. wodPrs];
    }
}
