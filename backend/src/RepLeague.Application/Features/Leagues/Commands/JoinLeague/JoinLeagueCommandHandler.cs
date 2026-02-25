using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Leagues.DTOs;
using RepLeague.Domain.Entities;
using RepLeague.Domain.Enums;

namespace RepLeague.Application.Features.Leagues.Commands.JoinLeague;

public class JoinLeagueCommandHandler(IAppDbContext db)
    : IRequestHandler<JoinLeagueCommand, LeagueDto>
{
    public async Task<LeagueDto> Handle(JoinLeagueCommand request, CancellationToken ct)
    {
        var invitation = await db.Invitations
            .Include(i => i.League)
            .FirstOrDefaultAsync(i => i.Token == request.Token, ct)
            ?? throw new NotFoundException("Invitation", request.Token);

        if (invitation.Status != InvitationStatus.Pending)
            throw new AppException("This invitation has already been used.");

        if (invitation.ExpiresAt < DateTime.UtcNow)
            throw new AppException("This invitation has expired.");

        // Check user is not already a member
        var alreadyMember = await db.LeagueMembers
            .AnyAsync(m => m.LeagueId == invitation.LeagueId && m.UserId == request.UserId, ct);

        if (alreadyMember)
            throw new ConflictException("You are already a member of this league.");

        invitation.Status = InvitationStatus.Accepted;

        var membership = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = invitation.LeagueId,
            UserId = request.UserId,
            JoinedAt = DateTime.UtcNow
        };

        var rankingEntry = new RankingEntry
        {
            Id = Guid.NewGuid(),
            LeagueId = invitation.LeagueId,
            UserId = request.UserId,
            Points = 0,
            UpdatedAt = DateTime.UtcNow
        };

        db.LeagueMembers.Add(membership);
        db.RankingEntries.Add(rankingEntry);
        await db.SaveChangesAsync(ct);

        var memberCount = await db.LeagueMembers
            .CountAsync(m => m.LeagueId == invitation.LeagueId, ct);

        var l = invitation.League;
        return new LeagueDto(
            l.Id, l.OwnerUserId, l.Name, l.Description, l.ImageUrl,
            memberCount, l.OwnerUserId == request.UserId, l.CreatedAt);
    }
}
