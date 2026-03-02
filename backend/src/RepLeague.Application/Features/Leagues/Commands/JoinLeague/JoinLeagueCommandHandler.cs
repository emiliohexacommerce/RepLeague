using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Leagues.DTOs;
using RepLeague.Domain.Entities;
using RepLeague.Domain.Enums;

namespace RepLeague.Application.Features.Leagues.Commands.JoinLeague;

public class JoinLeagueCommandHandler(IAppDbContext db, IEmailService emailService)
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

        // Calculate historical points from workouts done before joining
        var historicalStats = await db.Workouts
            .Where(w => w.UserId == request.UserId)
            .GroupBy(w => w.UserId)
            .Select(g => new
            {
                WorkoutCount = g.Count(),
                PrCount = g.Count(w => w.IsPR)
            })
            .FirstOrDefaultAsync(ct);

        const int PointsPerWorkout = 10;
        const int PointsPerPr = 30;

        var workoutCount = historicalStats?.WorkoutCount ?? 0;
        var prCount = historicalStats?.PrCount ?? 0;
        var historicalPoints = workoutCount * PointsPerWorkout + prCount * PointsPerPr;

        var rankingEntry = new RankingEntry
        {
            Id = Guid.NewGuid(),
            LeagueId = invitation.LeagueId,
            UserId = request.UserId,
            Points = historicalPoints,
            WorkoutCount = workoutCount,
            PrCount = prCount,
            UpdatedAt = DateTime.UtcNow
        };

        db.LeagueMembers.Add(membership);
        db.RankingEntries.Add(rankingEntry);
        await db.SaveChangesAsync(ct);

        var memberCount = await db.LeagueMembers
            .CountAsync(m => m.LeagueId == invitation.LeagueId, ct);

        var l = invitation.League;

        // Notify league owner — fire-and-forget
        _ = NotifyOwnerAsync(l.OwnerUserId, request.UserId, l.Id, l.Name, ct);

        return new LeagueDto(
            l.Id, l.OwnerUserId, l.Name, l.Description, l.ImageUrl,
            memberCount, l.OwnerUserId == request.UserId, l.CreatedAt);
    }

    private async Task NotifyOwnerAsync(
        Guid ownerId, Guid newMemberId, Guid leagueId, string leagueName, CancellationToken ct)
    {
        try
        {
            var owner = await db.Users
                .Where(u => u.Id == ownerId)
                .Select(u => new { u.Email, u.DisplayName })
                .FirstOrDefaultAsync(ct);

            var newMemberName = await db.Users
                .Where(u => u.Id == newMemberId)
                .Select(u => u.DisplayName)
                .FirstOrDefaultAsync(ct) ?? "Un atleta";

            if (owner == null) return;

            var ownerFirstName = owner.DisplayName.Split(' ')[0];
            var leagueUrl = $"/leagues/{leagueId}";

            await emailService.SendInvitationAcceptedEmailAsync(
                owner.Email, ownerFirstName, newMemberName, leagueName, leagueUrl, ct);
        }
        catch
        {
            // Email failure must not affect the join flow
        }
    }
}
