using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;

namespace RepLeague.Application.Features.Strength.Commands.DeleteLiftSession;

public class DeleteLiftSessionCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteLiftSessionCommand>
{
    public async Task Handle(DeleteLiftSessionCommand request, CancellationToken ct)
    {
        var session = await db.LiftSessions
            .FirstOrDefaultAsync(s => s.Id == request.LiftSessionId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.LiftSession), request.LiftSessionId);

        if (session.UserId != request.UserId)
            throw new UnauthorizedException("You do not own this lift session.");

        session.IsDeleted = true;
        session.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
