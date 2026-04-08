namespace RepLeague.Domain.Entities;

public class League
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateOnly? PointsActivatedAt { get; set; }   // null = sistema no activado
    public bool BackfillCompleted { get; set; } = false;

    public User Owner { get; set; } = null!;
    public ICollection<LeagueMember> Members { get; set; } = [];
    public ICollection<Invitation> Invitations { get; set; } = [];
    public ICollection<RankingEntry> Rankings { get; set; } = [];
    public ICollection<DailyWod> DailyWods { get; set; } = [];
    public ICollection<DailyPoints> DailyPoints { get; set; } = [];
}
