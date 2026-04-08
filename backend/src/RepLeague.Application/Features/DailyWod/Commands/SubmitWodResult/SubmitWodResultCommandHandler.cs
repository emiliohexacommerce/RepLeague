using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.DailyWod.DTOs;
using RepLeague.Application.Features.Points.Commands.CalculateDailyPoints;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.DailyWod.Commands.SubmitWodResult;

public class SubmitWodResultCommandHandler(IAppDbContext db, IMediator mediator)
    : IRequestHandler<SubmitWodResultCommand, DailyWodResultDto>
{
    public async Task<DailyWodResultDto> Handle(SubmitWodResultCommand request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Verify WOD exists for today in this league
        var wod = await db.DailyWods
            .FirstOrDefaultAsync(w => w.LeagueId == request.LeagueId && w.Date == today, ct)
            ?? throw new NotFoundException("DailyWod", $"league {request.LeagueId} today");

        // Verify user hasn't already submitted a result
        var alreadySubmitted = await db.DailyWodResults
            .AnyAsync(r => r.DailyWodId == wod.Id && r.UserId == request.UserId, ct);
        if (alreadySubmitted)
            throw new ConflictException("Ya registraste un resultado para el WOD de hoy.");

        var result = new Domain.Entities.DailyWodResult
        {
            Id = Guid.NewGuid(),
            DailyWodId = wod.Id,
            UserId = request.UserId,
            ElapsedSeconds = request.ElapsedSeconds,
            RoundsCompleted = request.RoundsCompleted,
            TotalReps = request.TotalReps,
            IsRx = request.IsRx,
            DidNotFinish = request.DidNotFinish,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        result.ExerciseDetails = request.ExerciseDetails
            .Select(e => new DailyWodResultExercise
            {
                Id = Guid.NewGuid(),
                DailyWodResultId = result.Id,
                DailyWodExerciseId = e.DailyWodExerciseId,
                RepsCompleted = e.RepsCompleted,
                WeightUsedKg = e.WeightUsedKg,
                DurationSeconds = e.DurationSeconds,
                Notes = e.Notes?.Trim()
            }).ToList();

        db.DailyWodResults.Add(result);
        await db.SaveChangesAsync(ct);

        // Recalculate daily points for the user
        await mediator.Send(
            new CalculateDailyPointsCommand(request.UserId, request.LeagueId, today), ct);

        var userDisplayName = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => u.DisplayName)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        return ToDto(result, userDisplayName);
    }

    internal static DailyWodResultDto ToDto(Domain.Entities.DailyWodResult r, string userDisplayName) => new(
        r.Id,
        r.DailyWodId,
        r.UserId,
        userDisplayName,
        r.ElapsedSeconds,
        r.RoundsCompleted,
        r.TotalReps,
        r.IsRx,
        r.DidNotFinish,
        r.Notes,
        r.CreatedAt,
        r.ExerciseDetails
            .Select(e => new DailyWodResultExerciseDto(
                e.Id, e.DailyWodExerciseId, e.RepsCompleted, e.WeightUsedKg, e.DurationSeconds, e.Notes))
            .ToList()
    );
}
