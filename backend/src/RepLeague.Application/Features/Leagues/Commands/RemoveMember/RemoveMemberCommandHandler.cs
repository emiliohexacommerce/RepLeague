using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Leagues.Commands.RemoveMember;

public class RemoveMemberCommandHandler(IAppDbContext db) : IRequestHandler<RemoveMemberCommand>
{
    public async Task Handle(RemoveMemberCommand request, CancellationToken ct)
    {
        var league = await db.Leagues.FirstOrDefaultAsync(l => l.Id == request.LeagueId, ct)
            ?? throw new NotFoundException(nameof(League), request.LeagueId);

        if (league.OwnerUserId != request.RequesterId)
            throw new UnauthorizedException("Only the league owner can remove members.");

        if (request.MemberUserId == request.RequesterId)
            throw new AppException("The owner cannot be removed from the league.");

        var member = await db.LeagueMembers
            .FirstOrDefaultAsync(m => m.LeagueId == request.LeagueId
                                   && m.UserId == request.MemberUserId, ct)
            ?? throw new NotFoundException("League member", request.MemberUserId);

        var rankingEntry = await db.RankingEntries
            .FirstOrDefaultAsync(r => r.LeagueId == request.LeagueId
                                   && r.UserId == request.MemberUserId, ct);

        db.LeagueMembers.Remove(member);
        if (rankingEntry != null)
            db.RankingEntries.Remove(rankingEntry);

        await db.SaveChangesAsync(ct);
    }
}
