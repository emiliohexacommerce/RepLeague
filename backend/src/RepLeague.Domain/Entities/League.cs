namespace RepLeague.Domain.Entities;

public class League
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User Owner { get; set; } = null!;
    public ICollection<LeagueMember> Members { get; set; } = [];
    public ICollection<Invitation> Invitations { get; set; } = [];
    public ICollection<RankingEntry> Rankings { get; set; } = [];
}
