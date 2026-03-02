namespace RepLeague.Domain.Entities;

/// <summary>A strength training session (one visit to the gym).</summary>
public class LiftSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public string? Title { get; set; }
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<StrengthSet> Sets { get; set; } = [];
}
