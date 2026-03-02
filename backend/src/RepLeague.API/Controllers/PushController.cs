using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepLeague.Application.Features.Push.Commands.SendTest;
using RepLeague.Application.Features.Push.Commands.Subscribe;

namespace RepLeague.API.Controllers;

[Authorize]
public class PushController(IMediator mediator) : BaseApiController(mediator)
{
    /// <summary>
    /// Registers (or updates) a Web Push subscription for the authenticated user.
    /// Called by the Angular PushService after requestSubscription().
    /// </summary>
    [HttpPost("/api/push/subscribe")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Subscribe(
        [FromBody] SubscribeRequest request,
        CancellationToken ct)
    {
        await Mediator.Send(new SubscribePushCommand(
            UserId:  CurrentUserId,
            Endpoint: request.Endpoint,
            P256dh:   request.Keys?.P256dh ?? string.Empty,
            Auth:     request.Keys?.Auth   ?? string.Empty), ct);

        return NoContent();
    }

    /// <summary>
    /// Sends a test push notification to all subscribed devices of the current user.
    /// </summary>
    [HttpPost("/api/push/test")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SendTest(CancellationToken ct)
    {
        await Mediator.Send(new SendTestPushCommand(CurrentUserId), ct);
        return NoContent();
    }
}

// ── Request DTOs ──────────────────────────────────────────────────────────────

public record SubscribeRequest(string Endpoint, SubscribeKeys? Keys);
public record SubscribeKeys(string P256dh, string Auth);
