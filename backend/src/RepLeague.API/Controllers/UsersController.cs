using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepLeague.Application.Features.Users.Commands.UpdateProfile;
using RepLeague.Application.Features.Users.Commands.UploadAvatar;
using RepLeague.Application.Features.Users.DTOs;
using RepLeague.Application.Features.Users.Queries.GetMe;

namespace RepLeague.API.Controllers;

[Authorize]
public class UsersController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpGet("/api/me")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMeQuery(CurrentUserId), ct);
        return Ok(result);
    }

    [HttpPatch("/api/me")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdateProfileCommand(CurrentUserId, request.DisplayName, request.Country, request.Bio), ct);
        return Ok(result);
    }

    [HttpPost("/api/me/avatar")]
    [ProducesResponseType(typeof(AvatarUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(6 * 1024 * 1024)] // 6 MB max request
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { errors = new[] { "No file provided." } });

        await using var stream = file.OpenReadStream();

        var avatarUrl = await Mediator.Send(new UploadAvatarCommand(
            CurrentUserId,
            stream,
            file.ContentType,
            file.Length), ct);

        return Ok(new AvatarUploadResponse(avatarUrl));
    }
}

public record UpdateProfileRequest(string? DisplayName, string? Country, string? Bio);
public record AvatarUploadResponse(string AvatarUrl);
