using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Wod.Commands.CreateWodEntry;
using RepLeague.Application.Features.Wod.DTOs;

namespace RepLeague.Application.Features.Wod.Queries.GetWodHistory;

public class GetWodHistoryQueryHandler(IAppDbContext db)
    : IRequestHandler<GetWodHistoryQuery, List<WodEntryDto>>
{
    public async Task<List<WodEntryDto>> Handle(GetWodHistoryQuery request, CancellationToken ct)
    {
        var query = db.WodEntries
            .Include(e => e.Exercises)
            .Include(e => e.AmrapResult)
            .Include(e => e.EmomResult)
            .Where(e => e.UserId == request.UserId && !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Type))
            query = query.Where(e => e.Type == request.Type);

        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (Math.Max(request.Page, 1) - 1) * pageSize;

        var entries = await query
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        return entries.Select(CreateWodEntryCommandHandler.ToDto).ToList();
    }
}
