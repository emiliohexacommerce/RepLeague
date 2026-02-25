using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Auth.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken ct)
    {
        var emailTaken = await db.Users.AnyAsync(u => u.Email == request.Email.ToLower(), ct);
        if (emailTaken)
            throw new ConflictException("Email is already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower(),
            PasswordHash = passwordHasher.Hash(request.Password),
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);

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
