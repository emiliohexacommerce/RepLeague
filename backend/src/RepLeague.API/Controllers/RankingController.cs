using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepLeague.Application.Features.Leagues.DTOs;
using RepLeague.Application.Features.Leagues.Queries.GetLeagueMemberProfile;
using RepLeague.Application.Features.Points.DTOs;
using RepLeague.Application.Features.Points.Queries.GetLeaguePointsRanking;

namespace RepLeague.API.Controllers;

[Authorize]
[Route("api/leagues/{leagueId:guid}")]
public class RankingController(IMediator mediator) : BaseApiController(mediator)
{
    /// <summary>Get the points ranking for a league.</summary>
    [HttpGet("ranking/points")]
    [ProducesResponseType(typeof(List<LeaguePointsRankingEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPointsRanking(
        Guid leagueId,
        [FromQuery] string period = "daily",
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetLeaguePointsRankingQuery(leagueId, period, CurrentUserId), ct);
        return Ok(result);
    }

    /// <summary>Get the public profile of a league member.</summary>
    [HttpGet("members/{userId:guid}/profile")]
    [ProducesResponseType(typeof(LeagueMemberProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMemberProfile(
        Guid leagueId,
        Guid userId,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetLeagueMemberProfileQuery(leagueId, userId, CurrentUserId), ct);
        return Ok(result);
    }
}
