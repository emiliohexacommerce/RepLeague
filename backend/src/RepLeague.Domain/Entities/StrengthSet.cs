namespace RepLeague.Domain.Entities;

/// <summary>A single set performed during a lift session.</summary>
public class StrengthSet
{
    public Guid Id { get; set; }
    public Guid LiftSessionId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
    public bool IsWarmup { get; set; }
    public bool IsPr { get; set; }
    public decimal? OneRepMaxKg { get; set; }  // computed via Epley formula
    public string? Notes { get; set; }

    public LiftSession LiftSession { get; set; } = null!;
}
