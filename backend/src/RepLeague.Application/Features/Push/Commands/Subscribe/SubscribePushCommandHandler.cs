using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Push.Commands.Subscribe;

public class SubscribePushCommandHandler(IAppDbContext db)
    : IRequestHandler<SubscribePushCommand>
{
    public async Task Handle(SubscribePushCommand request, CancellationToken ct)
    {
        // Upsert by Endpoint: one device = one subscription row
        var existing = await db.PushSubscriptions
            .FirstOrDefaultAsync(p => p.Endpoint == request.Endpoint, ct);

        if (existing is not null)
        {
            // Update keys in case they changed (browser may rotate them)
            existing.UserId = request.UserId;
            existing.P256dh = request.P256dh;
            existing.Auth = request.Auth;
        }
        else
        {
            db.PushSubscriptions.Add(new PushSubscription
            {
                UserId = request.UserId,
                Endpoint = request.Endpoint,
                P256dh = request.P256dh,
                Auth = request.Auth,
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
