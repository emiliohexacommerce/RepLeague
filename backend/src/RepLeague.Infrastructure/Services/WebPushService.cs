using System.Text.Json;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;
using Microsoft.Extensions.Configuration;
using RepLeague.Application.Common.Interfaces;

namespace RepLeague.Infrastructure.Services;

public class WebPushService : IWebPushService
{
    private readonly PushServiceClient _client;
    private readonly VapidAuthentication _vapid;

    public WebPushService(IConfiguration config)
    {
        var publicKey  = config["Vapid:PublicKey"]  ?? string.Empty;
        var privateKey = config["Vapid:PrivateKey"] ?? string.Empty;
        var subject    = config["Vapid:Subject"]    ?? "mailto:admin@repleague.app";

        _vapid = new VapidAuthentication(publicKey, privateKey) { Subject = subject };
        _client = new PushServiceClient(new HttpClient())
        {
            DefaultAuthentication = _vapid
        };
    }

    public async Task SendAsync(
        string endpoint,
        string p256dh,
        string auth,
        string title,
        string body,
        string? url = null,
        CancellationToken ct = default)
    {
        var subscription = new PushSubscription
        {
            Endpoint = endpoint,
            Keys = new Dictionary<string, string>
            {
                ["p256dh"] = p256dh,
                ["auth"]   = auth
            }
        };

        var payload = JsonSerializer.Serialize(new
        {
            notification = new
            {
                title,
                body,
                icon  = "/assets/media/logos/icons/icon-192x192.png",
                badge = "/assets/media/logos/icons/icon-72x72.png",
                data  = new { url = url ?? "/dashboard" },
                vibrate = new[] { 200, 100, 200 }
            }
        });

        var message = new PushMessage(payload)
        {
            TimeToLive = 24 * 60 * 60 // 1 day TTL
        };

        try
        {
            await _client.RequestPushMessageDeliveryAsync(subscription, message, ct);
        }
        catch (Exception ex)
        {
            // Log and swallow — a failed push should not break the main request
            Console.WriteLine($"[WebPushService] Failed to send push: {ex.Message}");
        }
    }
}
