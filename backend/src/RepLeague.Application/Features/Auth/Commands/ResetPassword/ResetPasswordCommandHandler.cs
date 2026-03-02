using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;

namespace RepLeague.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler(IAppDbContext db, IPasswordHasher passwordHasher)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token, ct)
            ?? throw new AppException("El token de restablecimiento no es válido.");

        if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            throw new AppException("El token de restablecimiento ha expirado.");

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        await db.SaveChangesAsync(ct);
    }
}
