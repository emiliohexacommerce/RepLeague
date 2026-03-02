namespace RepLeague.Domain.Entities;

public class PushSubscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    /// <summary>Push endpoint URL provided by the browser.</summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>ECDH public key (keys.p256dh from the browser PushSubscription).</summary>
    public string P256dh { get; set; } = string.Empty;

    /// <summary>Authentication secret (keys.auth from the browser PushSubscription).</summary>
    public string Auth { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
