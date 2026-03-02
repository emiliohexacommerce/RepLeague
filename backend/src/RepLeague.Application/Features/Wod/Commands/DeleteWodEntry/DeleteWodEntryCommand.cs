using MediatR;

namespace RepLeague.Application.Features.Wod.Commands.DeleteWodEntry;

public record DeleteWodEntryCommand(Guid WodEntryId, Guid UserId) : IRequest;
