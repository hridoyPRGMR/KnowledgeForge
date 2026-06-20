using KnowledgeForge.Application.Configuration;
using KnowledgeForge.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace KnowledgeForge.Infrastructure.Services;

public class LocalStorageService(
    IOptions<StorageOptions> storageOptions) : IStorageService
{
    private readonly StorageOptions _storage = storageOptions.Value;

    public async Task UploadFileAsync(string blobName, Stream fileStream, CancellationToken ct = default)
    {
        // Combine your base path with the unique filename
        var fullPath = Path.Combine(_storage.BookPath, blobName);

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(fullPath);
        if (directory != null) Directory.CreateDirectory(directory);

        await using var fs = File.Create(fullPath);
        await fileStream.CopyToAsync(fs, ct);
    }

    public async Task<Stream> OpenReadAsync(string blobName, CancellationToken ct = default)
    {
        var fullPath = GetAbsolutePath(blobName);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Could not find book file at: {fullPath}");

        // Open it asynchronously as a stream
        return await Task.FromResult(File.OpenRead(fullPath));
    }

    private string GetAbsolutePath(string blobName)
    {
        if (_storage.BookPath.StartsWith("./") || _storage.BookPath.StartsWith(".\\"))
        {
            var baseDir = AppContext.BaseDirectory;
            var combinedBase = Path.GetFullPath(Path.Combine(baseDir, _storage.BookPath));
            return Path.Combine(combinedBase, blobName);
        }

        return Path.Combine(_storage.BookPath, blobName);
    }
}