using MediatR;
using RepLeague.Application.Features.Users.DTOs;

namespace RepLeague.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    string? DisplayName,
    string? Country,
    string? Bio
) : IRequest<ProfileDto>;
