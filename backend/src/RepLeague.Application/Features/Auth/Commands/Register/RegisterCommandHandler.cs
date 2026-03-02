using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Application.Features.Auth.DTOs;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IEmailService emailService,
    IConfiguration configuration,
    ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, AuthResponseDto>
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

        // Email verification token (6-digit code, valid 24h)
        user.EmailVerificationToken = Random.Shared.Next(100000, 999999).ToString();
        user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

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

        // Send welcome + verification emails — non-blocking
        _ = Task.Run(async () =>
        {
            try
            {
                await emailService.SendWelcomeEmailAsync(user.Email, user.DisplayName);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Welcome email could not be sent to {Email}", user.Email);
            }
        });

        var frontendBaseUrl = configuration["FrontendBaseUrl"] ?? "http://localhost:4200";
        _ = Task.Run(async () =>
        {
            try
            {
                var verifyUrl = $"{frontendBaseUrl}/auth/verify-email?token={user.EmailVerificationToken}";
                await emailService.SendEmailVerificationAsync(
                    user.Email, user.DisplayName.Split(' ')[0],
                    user.EmailVerificationToken!, verifyUrl);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Verification email could not be sent to {Email}", user.Email);
            }
        });

        return new AuthResponseDto(
            tokenService.GenerateAccessToken(user),
            refreshToken.Token,
            tokenService.AccessTokenExpirationMinutes * 60,
            new UserDto(user.Id.ToString(), user.Email, user.DisplayName, user.AvatarUrl, user.Country, user.Bio)
        );
    }
}
