namespace RepLeague.Domain.Entities;

/// <summary>
/// A manually-entered personal record for a barbell exercise.
/// Each entry is a snapshot — all entries are kept to build history.
/// </summary>
public class ManualLiftPr
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    /// <summary>Standardized exercise name (from CrossFit barbell list).</summary>
    public string ExerciseName { get; set; } = null!;

    /// <summary>Weight stored always in kilograms.</summary>
    public decimal WeightKg { get; set; }

    public string? Notes { get; set; }

    /// <summary>Date when this PR was achieved.</summary>
    public DateOnly AchievedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
