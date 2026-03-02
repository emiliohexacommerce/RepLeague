using MediatR;
using Microsoft.AspNetCore.Mvc;
using RepLeague.Application.Features.Auth.Commands.ForgotPassword;
using RepLeague.Application.Features.Auth.Commands.Login;
using RepLeague.Application.Features.Auth.Commands.Refresh;
using RepLeague.Application.Features.Auth.Commands.Register;
using RepLeague.Application.Features.Auth.Commands.ResetPassword;
using RepLeague.Application.Features.Auth.Commands.VerifyEmail;
using RepLeague.Application.Features.Auth.DTOs;

namespace RepLeague.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>Sends a password reset email. Always returns 200 to prevent user enumeration.</summary>
    [HttpPost("forgot")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return Ok(new { message = "Si el email está registrado recibirás un enlace para restablecer tu contraseña." });
    }

    /// <summary>Resets the user's password using the token from the reset email.</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return Ok(new { message = "Contraseña restablecida exitosamente." });
    }

    /// <summary>Marks the user's email as verified using the 6-digit token from the verification email.</summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return Ok(new { message = "Email verificado exitosamente." });
    }
}
