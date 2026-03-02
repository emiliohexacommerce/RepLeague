using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Strength.Commands.CreateLiftSession;
using RepLeague.Application.Features.Strength.DTOs;

namespace RepLeague.Application.Features.Strength.Queries.GetLiftHistory;

public class GetLiftHistoryQueryHandler(IAppDbContext db)
    : IRequestHandler<GetLiftHistoryQuery, List<LiftSessionDto>>
{
    public async Task<List<LiftSessionDto>> Handle(GetLiftHistoryQuery request, CancellationToken ct)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (Math.Max(request.Page, 1) - 1) * pageSize;

        var sessions = await db.LiftSessions
            .Include(s => s.Sets)
            .Where(s => s.UserId == request.UserId)
            .OrderByDescending(s => s.Date)
            .ThenByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        return sessions.Select(CreateLiftSessionCommandHandler.ToDto).ToList();
    }
}
