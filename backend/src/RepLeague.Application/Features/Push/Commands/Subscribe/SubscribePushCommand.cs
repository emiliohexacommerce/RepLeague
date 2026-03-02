using MediatR;

namespace RepLeague.Application.Features.Push.Commands.Subscribe;

/// <param name="UserId">Authenticated user's ID (from JWT, set by controller).</param>
/// <param name="Endpoint">Browser push endpoint URL.</param>
/// <param name="P256dh">Browser ECDH public key.</param>
/// <param name="Auth">Browser auth secret.</param>
public record SubscribePushCommand(
    Guid UserId,
    string Endpoint,
    string P256dh,
    string Auth
) : IRequest;
