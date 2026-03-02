using MediatR;

namespace RepLeague.Application.Features.Strength.Commands.DeleteLiftSession;

public record DeleteLiftSessionCommand(Guid LiftSessionId, Guid UserId) : IRequest;
