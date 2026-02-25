namespace RepLeague.Application.Common.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadAvatarAsync(Guid userId, Stream fileStream, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string blobUrl, CancellationToken ct = default);
}
