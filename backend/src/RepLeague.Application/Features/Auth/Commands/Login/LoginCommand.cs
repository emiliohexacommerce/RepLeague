using MediatR;
using RepLeague.Application.Features.Auth.DTOs;

namespace RepLeague.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
