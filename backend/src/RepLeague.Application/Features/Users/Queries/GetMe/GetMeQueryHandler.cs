using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Users.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Users.Queries.GetMe;

public class GetMeQueryHandler(IAppDbContext db) : IRequestHandler<GetMeQuery, ProfileDto>
{
    public async Task<ProfileDto> Handle(GetMeQuery request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var totalWorkouts = await db.Workouts.CountAsync(w => w.UserId == request.UserId, ct);
        var totalPrs = await db.Workouts.CountAsync(w => w.UserId == request.UserId && w.IsPR, ct);
        var leagueCount = await db.LeagueMembers.CountAsync(m => m.UserId == request.UserId, ct);

        return new ProfileDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.Country,
            user.Bio,
            user.CreatedAt,
            new UserStatsDto(totalWorkouts, totalPrs, leagueCount)
        );
    }
}
