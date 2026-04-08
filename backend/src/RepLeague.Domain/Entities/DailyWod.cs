namespace RepLeague.Domain.Entities;

/// <summary>WOD del día seteado por el primer atleta que registra sesión en la liga.</summary>
public class DailyWod
{
    public Guid Id { get; set; }
    public Guid LeagueId { get; set; }
    public Guid SetByUserId { get; set; }          // primer atleta del día
    public DateOnly Date { get; set; }
    public string Type { get; set; } = string.Empty; // ForTime|AMRAP|EMOM|Chipper|Intervals
    public string Title { get; set; } = string.Empty;
    public int? TimeCapSeconds { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public League League { get; set; } = null!;
    public User SetByUser { get; set; } = null!;
    public ICollection<DailyWodExercise> Exercises { get; set; } = [];
    public ICollection<DailyWodResult> Results { get; set; } = [];
}
