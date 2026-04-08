namespace RepLeague.Domain.Entities;

/// <summary>Puntos ganados por un atleta en un día dentro de una liga.</summary>
public class DailyPoints
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LeagueId { get; set; }
    public DateOnly Date { get; set; }

    // Breakdown de puntos (máximo 10 por día)
    public int AttendancePoints { get; set; }      // +1 registró sesión
    public int VolumePoints { get; set; }          // +1 volumen > promedio histórico
    public int PrPoints { get; set; }              // +2 superó 1RM
    public int WodCompletionPoints { get; set; }   // +2 completó el WOD del día
    public int WodRankingPoints { get; set; }      // +2 fue el mejor del WOD del día
    public int StreakPoints { get; set; }          // +2 racha de 3+ días consecutivos

    public int TotalPoints => AttendancePoints + VolumePoints + PrPoints
                            + WodCompletionPoints + WodRankingPoints + StreakPoints;

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public League League { get; set; } = null!;
}
