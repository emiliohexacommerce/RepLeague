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
        // Each manual PR entry is an individual data point on the progress chart
        var points = await db.ManualLiftPrs
            .Where(x => x.UserId == request.UserId
                     && x.ExerciseName == request.Exercise
                     && !x.IsDeleted)
            .OrderBy(x => x.AchievedAt)
            .Select(x => new StrengthChartPointDto(x.AchievedAt, x.WeightKg))
            .ToListAsync(ct);

        return points;
    }
}
