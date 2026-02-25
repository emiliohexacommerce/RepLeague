using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Leagues.DTOs;

namespace RepLeague.Application.Features.Leagues.Queries.GetLeagueMembers;

public class GetLeagueMembersQueryHandler(IAppDbContext db)
    : IRequestHandler<GetLeagueMembersQuery, List<LeagueMemberDto>>
{
    public async Task<List<LeagueMemberDto>> Handle(GetLeagueMembersQuery request, CancellationToken ct)
    {
        return await db.LeagueMembers
            .Where(m => m.LeagueId == request.LeagueId)
            .Include(m => m.User)
            .OrderBy(m => m.JoinedAt)
            .Select(m => new LeagueMemberDto(
                m.UserId,
                m.User.DisplayName,
                m.User.AvatarUrl,
                m.JoinedAt))
            .ToListAsync(ct);
    }
}
