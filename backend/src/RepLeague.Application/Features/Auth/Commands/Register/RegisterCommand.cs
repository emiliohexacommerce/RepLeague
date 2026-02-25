using MediatR;
using RepLeague.Application.Features.Auth.DTOs;

namespace RepLeague.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string DisplayName
) : IRequest<AuthResponseDto>;
