using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Auth.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), ct)
            ?? throw new UnauthorizedException("Invalid credentials.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials.");

        var oldTokens = await db.RefreshTokens
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var old in oldTokens)
            old.IsRevoked = true;

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(ct);

        return new AuthResponseDto(
            tokenService.GenerateAccessToken(user),
            refreshToken.Token,
            tokenService.AccessTokenExpirationMinutes * 60,
            new UserDto(user.Id.ToString(), user.Email, user.DisplayName, user.AvatarUrl, user.Country, user.Bio)
        );
    }
}
