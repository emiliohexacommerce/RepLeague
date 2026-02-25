using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Leagues.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Leagues.Commands.CreateLeague;

public class CreateLeagueCommandHandler(IAppDbContext db)
    : IRequestHandler<CreateLeagueCommand, LeagueDto>
{
    public async Task<LeagueDto> Handle(CreateLeagueCommand request, CancellationToken ct)
    {
        var ownerExists = await db.Users.AnyAsync(u => u.Id == request.OwnerId, ct);
        if (!ownerExists)
            throw new NotFoundException(nameof(User), request.OwnerId);

        var league = new League
        {
            Id = Guid.NewGuid(),
            OwnerUserId = request.OwnerId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        // Owner is automatically a member
        var membership = new LeagueMember
        {
            Id = Guid.NewGuid(),
            LeagueId = league.Id,
            UserId = request.OwnerId,
            JoinedAt = DateTime.UtcNow
        };

        // Owner starts with a ranking entry at 0
        var rankingEntry = new RankingEntry
        {
            Id = Guid.NewGuid(),
            LeagueId = league.Id,
            UserId = request.OwnerId,
            Points = 0,
            UpdatedAt = DateTime.UtcNow
        };

        db.Leagues.Add(league);
        db.LeagueMembers.Add(membership);
        db.RankingEntries.Add(rankingEntry);
        await db.SaveChangesAsync(ct);

        return new LeagueDto(
            league.Id, league.OwnerUserId, league.Name,
            league.Description, league.ImageUrl, 1, true, league.CreatedAt);
    }
}
