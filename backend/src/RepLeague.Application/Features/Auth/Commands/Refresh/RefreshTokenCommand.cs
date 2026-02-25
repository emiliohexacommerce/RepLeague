using MediatR;
using RepLeague.Application.Features.Auth.DTOs;

namespace RepLeague.Application.Features.Auth.Commands.Refresh;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;
