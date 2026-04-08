namespace RepLeague.Application.Features.DailyWod.DTOs;

public record DailyWodExerciseDto(
    Guid Id,
    int Order,
    string ExerciseName,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds,
    string? Notes
);

public record DailyWodDto(
    Guid Id,
    Guid LeagueId,
    Guid SetByUserId,
    string SetByUserName,
    DateOnly Date,
    string Type,
    string Title,
    int? TimeCapSeconds,
    string? Notes,
    DateTime CreatedAt,
    List<DailyWodExerciseDto> Exercises
);

public record DailyWodResultExerciseDto(
    Guid Id,
    Guid DailyWodExerciseId,
    int? RepsCompleted,
    decimal? WeightUsedKg,
    int? DurationSeconds,
    string? Notes
);

public record DailyWodResultDto(
    Guid Id,
    Guid DailyWodId,
    Guid UserId,
    string UserDisplayName,
    int? ElapsedSeconds,
    int? RoundsCompleted,
    int? TotalReps,
    bool IsRx,
    bool DidNotFinish,
    string? Notes,
    DateTime CreatedAt,
    List<DailyWodResultExerciseDto> ExerciseDetails
);

public record ActivateLeaguePointsResult(
    DateOnly ActivatedAt,
    int BackfilledDays
);

public record DailyWodExerciseRequest(
    string ExerciseName,
    int? Reps,
    decimal? WeightKg,
    int? DurationSeconds,
    string? Notes
);

public record WodResultExerciseRequest(
    Guid DailyWodExerciseId,
    int? RepsCompleted,
    decimal? WeightUsedKg,
    int? DurationSeconds,
    string? Notes
);
