using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;

namespace RepLeague.Application.Features.Push.Commands.SendTest;

public class SendTestPushCommandHandler(IAppDbContext db, IWebPushService webPush)
    : IRequestHandler<SendTestPushCommand>
{
    public async Task Handle(SendTestPushCommand request, CancellationToken ct)
    {
        var subscriptions = await db.PushSubscriptions
            .Where(p => p.UserId == request.UserId)
            .ToListAsync(ct);

        foreach (var sub in subscriptions)
        {
            await webPush.SendAsync(
                endpoint: sub.Endpoint,
                p256dh: sub.P256dh,
                auth: sub.Auth,
                title: "¡RepLeague funcionando!",
                body: "Las notificaciones push están activas en tu dispositivo.",
                url: "/dashboard",
                ct: ct);
        }
    }
}
