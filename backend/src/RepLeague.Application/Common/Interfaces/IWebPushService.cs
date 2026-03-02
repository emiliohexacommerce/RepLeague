namespace RepLeague.Application.Common.Interfaces;

public interface IWebPushService
{
    /// <summary>
    /// Sends a Web Push notification to a specific subscription endpoint.
    /// </summary>
    Task SendAsync(
        string endpoint,
        string p256dh,
        string auth,
        string title,
        string body,
        string? url = null,
        CancellationToken ct = default);
}
