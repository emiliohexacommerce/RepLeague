using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RepLeague.Application.Common.Interfaces;

namespace RepLeague.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(
    IAppDbContext db, IEmailService emailService, IConfiguration configuration)
    : IRequestHandler<ForgotPasswordCommand>
{
    private const int ExpiryMinutes = 30;
    private readonly string _frontendBaseUrl =
        configuration["FrontendBaseUrl"] ?? "http://localhost:4200";

    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        // Always return without error to prevent user enumeration
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), ct);

        if (user == null) return;

        var token = Guid.NewGuid().ToString("N");
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(ExpiryMinutes);

        await db.SaveChangesAsync(ct);

        var resetUrl = $"{_frontendBaseUrl}/auth/reset-password?token={token}";
        var firstName = user.DisplayName.Split(' ')[0];

        try
        {
            await emailService.SendPasswordResetEmailAsync(
                user.Email, firstName, resetUrl, ExpiryMinutes, ct);
        }
        catch
        {
            // Email failure must not reveal whether the user exists
        }
    }
}
