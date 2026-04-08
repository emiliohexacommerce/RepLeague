using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.DailyWod.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.DailyWod.Commands.SetDailyWod;

public class SetDailyWodCommandHandler(IAppDbContext db)
    : IRequestHandler<SetDailyWodCommand, DailyWodDto>
{
    public async Task<DailyWodDto> Handle(SetDailyWodCommand request, CancellationToken ct)
    {
        // Verify user is member of league
        var isMember = await db.LeagueMembers
            .AnyAsync(m => m.LeagueId == request.LeagueId && m.UserId == request.UserId, ct);
        if (!isMember)
            throw new UnauthorizedException("El usuario no es miembro de esta liga.");

        // Verify league has points activated
        var league = await db.Leagues
            .FirstOrDefaultAsync(l => l.Id == request.LeagueId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.League), request.LeagueId);

        if (!league.PointsActivatedAt.HasValue)
            throw new AppException("El sistema de puntos no está activado en esta liga.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Verify no WOD exists for today
        var existingWod = await db.DailyWods
            .AnyAsync(w => w.LeagueId == request.LeagueId && w.Date == today, ct);
        if (existingWod)
            throw new ConflictException("Ya existe un WOD del día para esta liga hoy.");

        var wod = new Domain.Entities.DailyWod
        {
            Id = Guid.NewGuid(),
            LeagueId = request.LeagueId,
            SetByUserId = request.UserId,
            Date = today,
            Type = request.Type,
            Title = request.Title.Trim(),
            TimeCapSeconds = request.TimeCapSeconds,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        wod.Exercises = request.Exercises
            .Select((e, index) => new DailyWodExercise
            {
                Id = Guid.NewGuid(),
                DailyWodId = wod.Id,
                Order = index + 1,
                ExerciseName = e.ExerciseName.Trim(),
                Reps = e.Reps,
                WeightKg = e.WeightKg,
                DurationSeconds = e.DurationSeconds,
                Notes = e.Notes?.Trim()
            }).ToList();

        db.DailyWods.Add(wod);
        await db.SaveChangesAsync(ct);

        var setByUser = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        return ToDto(wod, setByUser);
    }

    internal static DailyWodDto ToDto(Domain.Entities.DailyWod wod, string setByUserName) => new(
        wod.Id,
        wod.LeagueId,
        wod.SetByUserId,
        setByUserName,
        wod.Date,
        wod.Type,
        wod.Title,
        wod.TimeCapSeconds,
        wod.Notes,
        wod.CreatedAt,
        wod.Exercises.OrderBy(e => e.Order)
            .Select(e => new DailyWodExerciseDto(
                e.Id, e.Order, e.ExerciseName, e.Reps, e.WeightKg, e.DurationSeconds, e.Notes))
            .ToList()
    );
}
