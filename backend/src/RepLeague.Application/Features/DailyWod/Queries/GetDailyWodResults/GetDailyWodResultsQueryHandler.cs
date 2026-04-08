using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.DailyWod.Commands.SubmitWodResult;
using RepLeague.Application.Features.DailyWod.DTOs;

namespace RepLeague.Application.Features.DailyWod.Queries.GetDailyWodResults;

public class GetDailyWodResultsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetDailyWodResultsQuery, List<DailyWodResultDto>>
{
    public async Task<List<DailyWodResultDto>> Handle(GetDailyWodResultsQuery request, CancellationToken ct)
    {
        // Verify user is member of league
        var isMember = await db.LeagueMembers
            .AnyAsync(m => m.LeagueId == request.LeagueId && m.UserId == request.RequestingUserId, ct);
        if (!isMember)
            throw new UnauthorizedException("El usuario no es miembro de esta liga.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var wod = await db.DailyWods
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.LeagueId == request.LeagueId && w.Date == today, ct);

        if (wod == null) return [];

        var results = await db.DailyWodResults
            .Include(r => r.User)
            .Include(r => r.ExerciseDetails)
            .Where(r => r.DailyWodId == wod.Id)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);

        return results
            .Select(r => SubmitWodResultCommandHandler.ToDto(r, r.User.DisplayName))
            .ToList();
    }
}
