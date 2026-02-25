using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Auth.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Auth.Commands.Refresh;

public class RefreshTokenCommandHandler(
    IAppDbContext db,
    ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var existing = await db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, ct)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (existing.IsRevoked || existing.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token expired or revoked.");

        existing.IsRevoked = true;

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = existing.UserId,
            Token = tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync(ct);

        return new AuthResponseDto(
            tokenService.GenerateAccessToken(existing.User),
            newRefreshToken.Token,
            tokenService.AccessTokenExpirationMinutes * 60,
            new UserDto(
                existing.User.Id.ToString(),
                existing.User.Email,
                existing.User.DisplayName,
                existing.User.AvatarUrl,
                existing.User.Country,
                existing.User.Bio)
        );
    }
}
