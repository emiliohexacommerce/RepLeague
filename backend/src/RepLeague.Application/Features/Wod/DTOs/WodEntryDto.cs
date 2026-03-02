namespace RepLeague.Application.Features.Wod.DTOs;

public record WodEntryDto(
    Guid Id,
    string Type,
    string? Title,
    DateOnly Date,
    string? TimeCap,
    string? ElapsedTime,
    int? Rounds,
    bool RxScaled,
    string? Notes,
    DateTime CreatedAt,
    List<WodExerciseDto> Exercises,
    WodResultAmrapDto? AmrapResult,
    WodResultEmomDto? EmomResult
);

public record WodExerciseDto(
    Guid Id,
    int OrderIndex,
    string Name,
    string MovementType,
    decimal? LoadValue,
    string? LoadUnit,
    int? Reps,
    string? Notes
);

public record WodResultAmrapDto(int RoundsCompleted, int ExtraReps);

public record WodResultEmomDto(int TotalMinutes, int IntervalsDone);
