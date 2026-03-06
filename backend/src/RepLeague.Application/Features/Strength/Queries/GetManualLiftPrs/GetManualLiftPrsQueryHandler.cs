using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;

namespace RepLeague.Application.Features.Strength.Queries.GetManualLiftPrs;

public class GetManualLiftPrsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetManualLiftPrsQuery, List<ManualLiftPrGroupDto>>
{
    public async Task<List<ManualLiftPrGroupDto>> Handle(GetManualLiftPrsQuery request, CancellationToken ct)
    {
        var entries = await db.ManualLiftPrs
            .AsNoTracking()
            .Where(x => x.UserId == request.UserId && !x.IsDeleted)
            .OrderByDescending(x => x.AchievedAt)
            .ToListAsync(ct);

        var groups = entries
            .GroupBy(x => x.ExerciseName)
            .Select(g =>
            {
                var best = g.MaxBy(x => x.WeightKg)!;
                var history = g
                    .OrderByDescending(x => x.AchievedAt)
                    .Select(x => new ManualLiftPrHistoryItem(x.Id, x.WeightKg, x.Notes, x.AchievedAt))
                    .ToList();

                return new ManualLiftPrGroupDto(g.Key, best.WeightKg, best.AchievedAt, history);
            })
            .OrderBy(g => g.ExerciseName)
            .ToList();

        return groups;
    }
}
