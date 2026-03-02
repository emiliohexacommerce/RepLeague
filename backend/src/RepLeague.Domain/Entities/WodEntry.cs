namespace RepLeague.Domain.Entities;

public class WodEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;   // ForTime|AMRAP|EMOM|Chipper|Intervals
    public string? Title { get; set; }
    public DateOnly Date { get; set; }
    public int? TimeCapSeconds { get; set; }
    public int? ElapsedSeconds { get; set; }
    public int? Rounds { get; set; }
    public bool RxScaled { get; set; } = true;
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<WodExercise> Exercises { get; set; } = [];
    public WodResultAmrap? AmrapResult { get; set; }
    public WodResultEmom? EmomResult { get; set; }
}
