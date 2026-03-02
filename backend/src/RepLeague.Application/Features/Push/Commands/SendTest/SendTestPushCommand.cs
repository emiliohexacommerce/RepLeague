using MediatR;

namespace RepLeague.Application.Features.Push.Commands.SendTest;

/// <param name="UserId">Sends a test notification to all subscriptions for this user.</param>
public record SendTestPushCommand(Guid UserId) : IRequest;
