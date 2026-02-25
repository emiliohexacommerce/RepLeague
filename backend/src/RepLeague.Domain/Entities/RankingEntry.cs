namespace RepLeague.Domain.Entities;

public class RankingEntry
{
    public Guid Id { get; set; }
    public Guid LeagueId { get; set; }
    public Guid UserId { get; set; }
    public int Points { get; set; }
    public int WorkoutCount { get; set; }
    public int PrCount { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public League League { get; set; } = null!;
    public User User { get; set; } = null!;
}
