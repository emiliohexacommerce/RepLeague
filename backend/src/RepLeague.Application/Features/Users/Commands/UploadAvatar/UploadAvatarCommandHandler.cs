using MediatR;
using Microsoft.EntityFrameworkCore;
using RepLeague.Application.Common.Exceptions;
using RepLeague.Application.Common.Interfaces;
using RepLeague.Domain.Entities;

namespace RepLeague.Application.Features.Users.Commands.UploadAvatar;

public class UploadAvatarCommandHandler(IAppDbContext db, IBlobStorageService blobStorage)
    : IRequestHandler<UploadAvatarCommand, string>
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    public async Task<string> Handle(UploadAvatarCommand request, CancellationToken ct)
    {
        if (request.FileSizeBytes > MaxFileSizeBytes)
            throw new AppException("Avatar file must be 5 MB or less.");

        if (!AllowedContentTypes.Contains(request.ContentType))
            throw new AppException("Only JPEG, PNG and WebP images are accepted.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        // Delete previous avatar from blob storage if it exists
        if (user.AvatarUrl != null)
            await blobStorage.DeleteAsync(user.AvatarUrl, ct);

        var newUrl = await blobStorage.UploadAvatarAsync(
            request.UserId, request.FileStream, request.ContentType, ct);

        user.AvatarUrl = newUrl;
        await db.SaveChangesAsync(ct);

        return newUrl;
    }
}
