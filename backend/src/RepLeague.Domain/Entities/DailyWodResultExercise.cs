namespace RepLeague.Domain.Entities;

/// <summary>Desglose por ejercicio del resultado del atleta en el WOD del día.</summary>
public class DailyWodResultExercise
{
    public Guid Id { get; set; }
    public Guid DailyWodResultId { get; set; }
    public Guid DailyWodExerciseId { get; set; }
    public int? RepsCompleted { get; set; }
    public decimal? WeightUsedKg { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Notes { get; set; }

    public DailyWodResult DailyWodResult { get; set; } = null!;
    public DailyWodExercise DailyWodExercise { get; set; } = null!;
}
