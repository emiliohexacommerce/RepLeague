using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Leagues.DTOs;

namespace RepLeague.Application.Features.Leagues.Queries.GetMyLeagues;

public class GetMyLeaguesQueryHandler(IAppDbContext db)
    : IRequestHandler<GetMyLeaguesQuery, List<LeagueDto>>
{
    public async Task<List<LeagueDto>> Handle(GetMyLeaguesQuery request, CancellationToken ct)
    {
        // All leagues the user is a member of (includes leagues they own)
        var leagues = await db.LeagueMembers
            .Where(m => m.UserId == request.UserId)
            .Include(m => m.League)
            .Select(m => new
            {
                m.League,
                MemberCount = db.LeagueMembers.Count(x => x.LeagueId == m.LeagueId)
            })
            .OrderByDescending(x => x.League.CreatedAt)
            .ToListAsync(ct);

        return leagues.Select(x => new LeagueDto(
            x.League.Id,
            x.League.OwnerUserId,
            x.League.Name,
            x.League.Description,
            x.League.ImageUrl,
            x.MemberCount,
            x.League.OwnerUserId == request.UserId,
            x.League.CreatedAt
        )).ToList();
    }
}
