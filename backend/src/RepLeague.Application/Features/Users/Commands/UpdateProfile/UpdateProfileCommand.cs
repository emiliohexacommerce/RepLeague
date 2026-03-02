using MediatR;
using RepLeague.Application.Features.Users.DTOs;

namespace RepLeague.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    Guid UserId,
    string? DisplayName,
    string? Country,
    string? Bio,
    // Extended profile fields
    string? Phone,
    DateOnly? BirthDate,
    string? City,
    string? GymName,
    string? Units,
    string? OneRmMethod,
    string? Visibility,
    bool? MarketingConsent
) : IRequest<ProfileDto>;
