using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;

namespace RepLeague.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler(IAppDbContext db)
    : IRequestHandler<VerifyEmailCommand>
{
    public async Task Handle(VerifyEmailCommand request, CancellationToken ct)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == request.Token, ct)
            ?? throw new AppException("El token de verificación no es válido.");

        if (user.EmailVerificationTokenExpiry == null || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            throw new AppException("El token de verificación ha expirado. Solicita uno nuevo.");

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;

        await db.SaveChangesAsync(ct);
    }
}
