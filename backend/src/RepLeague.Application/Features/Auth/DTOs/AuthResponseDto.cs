namespace RepLeague.Application.Features.Auth.DTOs;

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserDto User
);

public record UserDto(
    string Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? Country,
    string? Bio
);
