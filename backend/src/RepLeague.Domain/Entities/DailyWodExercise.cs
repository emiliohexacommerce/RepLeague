namespace RepLeague.Domain.Entities;

/// <summary>Un ejercicio dentro del WOD del día.</summary>
public class DailyWodExercise
{
    public Guid Id { get; set; }
    public Guid DailyWodId { get; set; }
    public int Order { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int? Reps { get; set; }
    public decimal? WeightKg { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Notes { get; set; }

    public DailyWod DailyWod { get; set; } = null!;
}
