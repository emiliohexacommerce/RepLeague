using RepLeague.Domain.Enums;

namespace RepLeague.Application.Features.Workouts.DTOs;

public record WorkoutDto(
    Guid Id,
    WorkoutType Type,
    bool IsPR,
    string? Notes,
    DateTime CreatedAt,
    IReadOnlyList<ExerciseDto> Exercises,
    WodDto? Wod
);

public record ExerciseDto(
    Guid Id,
    string ExerciseName,
    int Sets,
    int Reps,
    decimal WeightKg
);

public record WodDto(
    string WodName,
    string? Duration,
    int? Rounds,
    int? TotalReps
);

public record CreateExerciseDto(
    string ExerciseName,
    int Sets,
    int Reps,
    decimal WeightKg
);

public record CreateWodDto(
    string WodName,
    string? Duration,
    int? Rounds,
    int? TotalReps
);
