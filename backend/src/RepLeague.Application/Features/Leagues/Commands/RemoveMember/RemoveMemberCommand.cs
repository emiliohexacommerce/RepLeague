using MediatR;

namespace RepLeague.Application.Features.Leagues.Commands.RemoveMember;

public record RemoveMemberCommand(
    Guid LeagueId,
    Guid RequesterId,
    Guid MemberUserId
) : IRequest;
