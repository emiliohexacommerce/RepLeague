using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Leagues.Commands.DeleteLeague;

public class DeleteLeagueCommandHandler(IAppDbContext db) : IRequestHandler<DeleteLeagueCommand>
{
    public async Task Handle(DeleteLeagueCommand request, CancellationToken ct)
    {
        var league = await db.Leagues.FirstOrDefaultAsync(l => l.Id == request.LeagueId, ct)
            ?? throw new NotFoundException(nameof(League), request.LeagueId);

        if (league.OwnerUserId != request.RequesterId)
            throw new UnauthorizedException("Only the league owner can delete the league.");

        db.Leagues.Remove(league);
        await db.SaveChangesAsync(ct);
    }
}
