namespace RepLeague.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Country { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Workout> Workouts { get; set; } = [];
    public ICollection<League> OwnedLeagues { get; set; } = [];
    public ICollection<LeagueMember> LeagueMemberships { get; set; } = [];
    public ICollection<RankingEntry> RankingEntries { get; set; } = [];
}
