using MediatR;
using RepLeague.Application.Features.Wod.DTOs;

namespace RepLeague.Application.Features.Wod.Commands.CreateWodEntry;

public record CreateWodEntryCommand(
    Guid UserId,
    string Type,
    string? Title,
    DateOnly Date,
    string? TimeCap,
    string? ElapsedTime,
    int? Rounds,
    bool RxScaled,
    string? Notes,
    List<CreateWodExerciseDto> Exercises,
    CreateWodResultAmrapDto? AmrapResult,
    CreateWodResultEmomDto? EmomResult
) : IRequest<WodEntryDto>;

public record CreateWodExerciseDto(
    int OrderIndex,
    string Name,
    string MovementType,
    decimal? LoadValue,
    string? LoadUnit,
    int? Reps,
    string? Notes
);

public record CreateWodResultAmrapDto(int RoundsCompleted, int ExtraReps);
public record CreateWodResultEmomDto(int TotalMinutes, int IntervalsDone);
