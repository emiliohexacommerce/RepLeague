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

    // Extended profile fields
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? City { get; set; }
    public string? GymName { get; set; }

    // Preferences
    public string Units { get; set; } = "kg";           // "kg" | "lb"
    public string OneRmMethod { get; set; } = "Epley";  // "Epley" | "Brzycki"

    // Privacy / consent
    public string Visibility { get; set; } = "leagues"; // "private" | "leagues" | "public"
    public bool MarketingConsent { get; set; } = false;

    // Email verification
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }

    // Password reset
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    public ICollection<Workout> Workouts { get; set; } = [];
    public ICollection<League> OwnedLeagues { get; set; } = [];
    public ICollection<LeagueMember> LeagueMemberships { get; set; } = [];
    public ICollection<RankingEntry> RankingEntries { get; set; } = [];
}
