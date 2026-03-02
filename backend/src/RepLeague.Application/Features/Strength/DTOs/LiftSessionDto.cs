namespace RepLeague.Application.Features.Strength.DTOs;

public record LiftSessionDto(
    Guid Id,
    DateOnly Date,
    string? Title,
    string? Notes,
    DateTime CreatedAt,
    List<StrengthSetDto> Sets
);

public record StrengthSetDto(
    Guid Id,
    string ExerciseName,
    int SetNumber,
    int Reps,
    decimal WeightKg,
    bool IsWarmup,
    bool IsPr,
    decimal? OneRepMaxKg,
    string? Notes
);
