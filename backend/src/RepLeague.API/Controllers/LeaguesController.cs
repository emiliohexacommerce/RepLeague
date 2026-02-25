using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepLeague.Application.Features.Leagues.Commands.CreateLeague;
using RepLeague.Application.Features.Leagues.Commands.DeleteLeague;
using RepLeague.Application.Features.Leagues.Commands.InviteMember;
using RepLeague.Application.Features.Leagues.Commands.JoinLeague;
using RepLeague.Application.Features.Leagues.Commands.RemoveMember;
using RepLeague.Application.Features.Leagues.DTOs;
using RepLeague.Application.Features.Leagues.Queries.GetLeagueById;
using RepLeague.Application.Features.Leagues.Queries.GetLeagueMembers;
using RepLeague.Application.Features.Leagues.Queries.GetLeagueRanking;
using RepLeague.Application.Features.Leagues.Queries.GetMyLeagues;

namespace RepLeague.API.Controllers;

[Authorize]
public class LeaguesController(IMediator mediator) : BaseApiController(mediator)
{
    // ── League CRUD ───────────────────────────────────────────────────────────

    [HttpPost]
    [ProducesResponseType(typeof(LeagueDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateLeagueRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CreateLeagueCommand(CurrentUserId, request.Name, request.Description), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LeagueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetLeagueByIdQuery(id, CurrentUserId), ct);
        return Ok(result);
    }

    [HttpGet("mine")]
    [ProducesResponseType(typeof(List<LeagueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMyLeaguesQuery(CurrentUserId), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteLeagueCommand(id, CurrentUserId), ct);
        return NoContent();
    }

    // ── Members & Invitations ─────────────────────────────────────────────────

    [HttpPost("{id:guid}/invite")]
    [ProducesResponseType(typeof(InvitationResultDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Invite(
        Guid id, [FromBody] InviteRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new InviteMemberCommand(id, CurrentUserId, request.Email), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("join/{token}")]
    [ProducesResponseType(typeof(LeagueDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Join(string token, CancellationToken ct)
    {
        var result = await Mediator.Send(new JoinLeagueCommand(token, CurrentUserId), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/members")]
    [ProducesResponseType(typeof(List<LeagueMemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetLeagueMembersQuery(id), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveMemberCommand(id, CurrentUserId, userId), ct);
        return NoContent();
    }

    // ── Ranking ───────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/ranking")]
    [ProducesResponseType(typeof(List<LeagueRankingEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRanking(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetLeagueRankingQuery(id), ct);
        return Ok(result);
    }
}

public record CreateLeagueRequest(string Name, string? Description);
public record InviteRequest(string? Email);
