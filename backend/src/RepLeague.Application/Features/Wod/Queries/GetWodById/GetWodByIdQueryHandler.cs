using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Wod.Commands.CreateWodEntry;
using RepLeague.Application.Features.Wod.DTOs;

namespace RepLeague.Application.Features.Wod.Queries.GetWodById;

public class GetWodByIdQueryHandler(IAppDbContext db)
    : IRequestHandler<GetWodByIdQuery, WodEntryDto>
{
    public async Task<WodEntryDto> Handle(GetWodByIdQuery request, CancellationToken ct)
    {
        var entry = await db.WodEntries
            .Include(e => e.Exercises)
            .Include(e => e.AmrapResult)
            .Include(e => e.EmomResult)
            .FirstOrDefaultAsync(e => e.Id == request.WodEntryId && !e.IsDeleted, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.WodEntry), request.WodEntryId);

        if (entry.UserId != request.UserId)
            throw new UnauthorizedException("You do not own this WOD entry.");

        return CreateWodEntryCommandHandler.ToDto(entry);
    }
}
