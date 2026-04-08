using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.DailyWod.Commands.SetDailyWod;
using RepLeague.Application.Features.DailyWod.DTOs;

namespace RepLeague.Application.Features.DailyWod.Queries.GetDailyWod;

public class GetDailyWodQueryHandler(IAppDbContext db)
    : IRequestHandler<GetDailyWodQuery, DailyWodDto?>
{
    public async Task<DailyWodDto?> Handle(GetDailyWodQuery request, CancellationToken ct)
    {
        // Verify user is member of league
        var isMember = await db.LeagueMembers
            .AnyAsync(m => m.LeagueId == request.LeagueId && m.UserId == request.RequestingUserId, ct);
        if (!isMember)
            throw new UnauthorizedException("El usuario no es miembro de esta liga.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var wod = await db.DailyWods
            .Include(w => w.Exercises)
            .Include(w => w.SetByUser)
            .FirstOrDefaultAsync(w => w.LeagueId == request.LeagueId && w.Date == today, ct);

        if (wod == null) return null;

        return SetDailyWodCommandHandler.ToDto(wod, wod.SetByUser.DisplayName);
    }
}
