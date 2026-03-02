using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Users.DTOs;

namespace RepLeague.Application.Features.Users.Queries.GetStrengthChart;

public class GetStrengthChartQueryHandler(IAppDbContext db)
    : IRequestHandler<GetStrengthChartQuery, List<StrengthChartPointDto>>
{
    public async Task<List<StrengthChartPointDto>> Handle(GetStrengthChartQuery request, CancellationToken ct)
    {
        var sets = await db.StrengthSets
            .Include(x => x.LiftSession)
            .Where(x => x.LiftSession.UserId == request.UserId
                     && x.ExerciseName == request.Exercise
                     && !x.IsWarmup
                     && x.OneRepMaxKg.HasValue)
            .ToListAsync(ct);

        // Per date: take the max 1RM achieved
        var points = sets
            .GroupBy(x => x.LiftSession.Date)
            .Select(g => new StrengthChartPointDto(
                g.Key,
                g.Max(x => x.OneRepMaxKg!.Value)
            ))
            .OrderBy(p => p.Date)
            .ToList();

        return points;
    }
}
