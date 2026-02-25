using MediatR;
using RepLeague.Application.Features.Leagues.DTOs;

namespace RepLeague.Application.Features.Leagues.Queries.GetLeagueMembers;

public record GetLeagueMembersQuery(Guid LeagueId) : IRequest<List<LeagueMemberDto>>;
