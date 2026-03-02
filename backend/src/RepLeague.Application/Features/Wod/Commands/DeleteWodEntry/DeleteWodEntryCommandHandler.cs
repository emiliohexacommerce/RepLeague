using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;

namespace RepLeague.Application.Features.Wod.Commands.DeleteWodEntry;

public class DeleteWodEntryCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteWodEntryCommand>
{
    public async Task Handle(DeleteWodEntryCommand request, CancellationToken ct)
    {
        var entry = await db.WodEntries
            .FirstOrDefaultAsync(e => e.Id == request.WodEntryId && !e.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.WodEntry), request.WodEntryId);

        if (entry.UserId != request.UserId)
            throw new UnauthorizedException("You do not own this WOD entry.");

        entry.IsDeleted = true;
        entry.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
