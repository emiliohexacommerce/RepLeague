using MediatR;

namespace RepLeague.Application.Features.Users.Commands.UploadAvatar;

public record UploadAvatarCommand(
    Guid UserId,
    Stream FileStream,
    string ContentType,
    long FileSizeBytes
) : IRequest<string>;  // returns the new AvatarUrl
