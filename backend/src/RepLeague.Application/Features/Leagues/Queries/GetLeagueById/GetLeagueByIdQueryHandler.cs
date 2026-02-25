using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Leagues.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Leagues.Queries.GetLeagueById;

public class GetLeagueByIdQueryHandler(IAppDbContext db)
    : IRequestHandler<GetLeagueByIdQuery, LeagueDto>
{
    public async Task<LeagueDto> Handle(GetLeagueByIdQuery request, CancellationToken ct)
    {
        var league = await db.Leagues
            .FirstOrDefaultAsync(l => l.Id == request.LeagueId, ct)
            ?? throw new NotFoundException(nameof(League), request.LeagueId);

        var memberCount = await db.LeagueMembers
            .CountAsync(m => m.LeagueId == request.LeagueId, ct);

        return new LeagueDto(
            league.Id, league.OwnerUserId, league.Name,
            league.Description, league.ImageUrl, memberCount,
            league.OwnerUserId == request.RequesterId, league.CreatedAt);
    }
}
