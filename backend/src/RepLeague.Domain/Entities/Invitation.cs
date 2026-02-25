using RepLeague.Domain.Enums;

namespace RepLeague.Domain.Entities;

public class Invitation
{
    public Guid Id { get; set; }
    public Guid LeagueId { get; set; }
    public string? Email { get; set; }
    public Guid? InvitedUserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    public League League { get; set; } = null!;
}
