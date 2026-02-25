using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Leagues.DTOs;

namespace RepLeague.Application.Features.Leagues.Queries.GetLeagueRanking;

public class GetLeagueRankingQueryHandler(IAppDbContext db)
    : IRequestHandler<GetLeagueRankingQuery, List<LeagueRankingEntryDto>>
{
    public async Task<List<LeagueRankingEntryDto>> Handle(GetLeagueRankingQuery request, CancellationToken ct)
    {
        var entries = await db.RankingEntries
            .Where(r => r.LeagueId == request.LeagueId)
            .Include(r => r.User)
            .OrderByDescending(r => r.Points)
            .ThenByDescending(r => r.PrCount)
            .ThenBy(r => r.UpdatedAt)
            .ToListAsync(ct);

        return entries.Select((entry, index) => new LeagueRankingEntryDto(
            index + 1,
            entry.UserId,
            entry.User.DisplayName,
            entry.User.AvatarUrl,
            entry.Points,
            entry.WorkoutCount,
            entry.PrCount
        )).ToList();
    }
}
