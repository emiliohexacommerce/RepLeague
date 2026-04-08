namespace RepLeague.Domain.Entities;

/// <summary>Resultado de un atleta en el WOD del día.</summary>
public class DailyWodResult
{
    public Guid Id { get; set; }
    public Guid DailyWodId { get; set; }
    public Guid UserId { get; set; }
    public int? ElapsedSeconds { get; set; }       // ForTime
    public int? RoundsCompleted { get; set; }      // AMRAP
    public int? TotalReps { get; set; }            // Chipper/general
    public bool IsRx { get; set; } = true;
    public bool DidNotFinish { get; set; } = false;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DailyWod DailyWod { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<DailyWodResultExercise> ExerciseDetails { get; set; } = [];
}
