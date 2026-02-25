using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Leagues.DTOs;
using RepLeague.Domain.Entities;
using RepLeague.Domain.Enums;

namespace RepLeague.Application.Features.Leagues.Commands.InviteMember;

public class InviteMemberCommandHandler(IAppDbContext db, IEmailService emailService)
    : IRequestHandler<InviteMemberCommand, InvitationResultDto>
{
    public async Task<InvitationResultDto> Handle(InviteMemberCommand request, CancellationToken ct)
    {
        var league = await db.Leagues.FirstOrDefaultAsync(l => l.Id == request.LeagueId, ct)
            ?? throw new NotFoundException(nameof(League), request.LeagueId);

        if (league.OwnerUserId != request.RequesterId)
            throw new UnauthorizedException("Only the league owner can invite members.");

        // Revoke any pending invite for the same email to this league
        if (request.Email != null)
        {
            var pending = await db.Invitations
                .Where(i => i.LeagueId == request.LeagueId
                         && i.Email == request.Email.ToLower()
                         && i.Status == InvitationStatus.Pending)
                .ToListAsync(ct);

            foreach (var old in pending)
                old.Status = InvitationStatus.Rejected;
        }

        var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            LeagueId = request.LeagueId,
            Email = request.Email?.ToLower(),
            Token = token,
            Status = InvitationStatus.Pending,
            SentAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        db.Invitations.Add(invitation);
        await db.SaveChangesAsync(ct);

        var joinUrl = $"/leagues/join/{token}";

        // Send email if provided (fire-and-forget errors don't block the response)
        if (request.Email != null)
        {
            try
            {
                await emailService.SendLeagueInvitationAsync(
                    request.Email, league.Name, joinUrl, ct);
            }
            catch
            {
                // Email failure should not block the invitation creation
            }
        }

        return new InvitationResultDto(invitation.Id, joinUrl, token);
    }
}
