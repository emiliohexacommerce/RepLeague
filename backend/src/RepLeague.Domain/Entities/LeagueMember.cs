namespace RepLeague.Domain.Entities;

public class LeagueMember
{
    public Guid Id { get; set; }
    public Guid LeagueId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public League League { get; set; } = null!;
    public User User { get; set; } = null!;
}
