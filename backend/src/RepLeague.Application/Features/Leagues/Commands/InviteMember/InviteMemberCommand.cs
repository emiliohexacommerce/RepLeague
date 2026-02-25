using MediatR;
using RepLeague.Application.Features.Leagues.DTOs;

namespace RepLeague.Application.Features.Leagues.Commands.InviteMember;

public record InviteMemberCommand(
    Guid LeagueId,
    Guid RequesterId,
    string? Email
) : IRequest<InvitationResultDto>;
