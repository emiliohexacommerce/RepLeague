using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;

namespace RepLeague.Application.Features.Strength.Queries.GetLiftPrs;

public class GetLiftPrsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetLiftPrsQuery, List<LiftPrDto>>
{
    public async Task<List<LiftPrDto>> Handle(GetLiftPrsQuery request, CancellationToken ct)
    {
        var sets = await db.StrengthSets
            .Include(x => x.LiftSession)
            .Where(x => x.LiftSession.UserId == request.UserId && !x.IsWarmup)
            .ToListAsync(ct);

        var prs = sets
            .GroupBy(x => x.ExerciseName)
            .Select(g =>
            {
                var best1rm = g.Where(x => x.OneRepMaxKg.HasValue)
                               .MaxBy(x => x.OneRepMaxKg);
                var bestWeight = g.MaxBy(x => x.WeightKg);

                var pivot = best1rm ?? bestWeight;

                return new LiftPrDto(
                    g.Key,
                    bestWeight?.WeightKg ?? 0,
                    bestWeight?.Reps ?? 0,
                    best1rm?.OneRepMaxKg,
                    pivot?.LiftSession.CreatedAt ?? DateTime.MinValue
                );
            })
            .OrderBy(p => p.ExerciseName)
            .ToList();

        return prs;
    }
}
