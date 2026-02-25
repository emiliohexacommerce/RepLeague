using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using RepLeague.Application.Common.Interfaces;

namespace RepLeague.Infrastructure.Services;

public class BlobStorageService(IConfiguration configuration) : IBlobStorageService
{
    private readonly string _connectionString = configuration["AzureStorage:ConnectionString"]
        ?? throw new InvalidOperationException("AzureStorage:ConnectionString not configured.");
    private readonly string _containerName = configuration["AzureStorage:ContainerName"] ?? "replague-media";

    public async Task<string> UploadAvatarAsync(
        Guid userId, Stream fileStream, string contentType, CancellationToken ct = default)
    {
        var container = await GetContainerAsync(ct);

        var blobName = $"avatars/{userId}/{Guid.NewGuid()}";
        var blobClient = container.GetBlobClient(blobName);

        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: ct);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string blobUrl, CancellationToken ct = default)
    {
        var container = await GetContainerAsync(ct);

        // Extract blob name from URL
        var uri = new Uri(blobUrl);
        var blobName = uri.AbsolutePath.TrimStart('/').Replace($"{_containerName}/", "");

        var blobClient = container.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
    }

    private async Task<BlobContainerClient> GetContainerAsync(CancellationToken ct)
    {
        var serviceClient = new BlobServiceClient(_connectionString);
        var container = serviceClient.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);
        return container;
    }
}
