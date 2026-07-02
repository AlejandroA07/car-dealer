using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using WestcoastCars.Web.Configurations;

namespace WestcoastCars.Web.Services;

// Web stores and serves uploaded vehicle photos itself (from its own wwwroot, same as the seeded
// car images) rather than routing through the Api, because the Api is deliberately kept off the
// public network in production (docker-compose.deploy.yml keeps `api` internal-only) — only `web`
// is a public entrypoint, so image URLs handed to the browser must resolve against `web`.
public partial class LocalImageUploadService(IOptions<ImageUploadOptions> options) : IImageUploadService
{
    private readonly ImageUploadOptions _options = options.Value;

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length > _options.MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"Filen är för stor. Max {_options.MaxFileSizeBytes / (1024 * 1024)} MB.");
        }

        using var buffer = new MemoryStream();
        await using (var stream = file.OpenReadStream())
        {
            await stream.CopyToAsync(buffer, cancellationToken);
        }
        buffer.Position = 0;

        var extension = DetectExtension(buffer.ToArray())
            ?? throw new InvalidOperationException("Filtypen stöds inte. Tillåtna format: JPEG, PNG, WEBP, AVIF.");

        Directory.CreateDirectory(_options.StoragePath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_options.StoragePath, fileName);

        buffer.Position = 0;
        await using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
        {
            await buffer.CopyToAsync(fileStream, cancellationToken);
        }

        return $"{_options.UrlPath}/{fileName}";
    }

    public Task DeleteIfOwnedAsync(string? imageUrl, CancellationToken cancellationToken)
    {
        var prefix = _options.UrlPath.EndsWith('/') ? _options.UrlPath : _options.UrlPath + "/";

        if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith(prefix, StringComparison.Ordinal))
        {
            return Task.CompletedTask;
        }

        var fileName = imageUrl[prefix.Length..];
        if (!GeneratedFileNameRegex().IsMatch(fileName))
        {
            return Task.CompletedTask;
        }

        var storageRoot = Path.GetFullPath(_options.StoragePath);
        var fullPath = Path.GetFullPath(Path.Combine(storageRoot, fileName));

        if (!fullPath.StartsWith(storageRoot, StringComparison.Ordinal))
        {
            return Task.CompletedTask;
        }

        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (IOException)
        {
            // Cleanup is best-effort; a locked/missing file shouldn't fail the vehicle update/delete.
        }
        catch (UnauthorizedAccessException)
        {
        }

        return Task.CompletedTask;
    }

    private static string? DetectExtension(byte[] bytes)
    {
        if (Matches(bytes, [0xFF, 0xD8, 0xFF], 0)) return ".jpg";
        if (Matches(bytes, [0x89, 0x50, 0x4E, 0x47], 0)) return ".png";

        if (bytes.Length >= 12 && Matches(bytes, "RIFF"u8.ToArray(), 0) && Matches(bytes, "WEBP"u8.ToArray(), 8))
        {
            return ".webp";
        }

        if (bytes.Length >= 12 && Matches(bytes, "ftyp"u8.ToArray(), 4))
        {
            var brand = Encoding.ASCII.GetString(bytes, 8, 4);
            if (brand is "avif" or "avis")
            {
                return ".avif";
            }
        }

        return null;
    }

    private static bool Matches(byte[] bytes, byte[] signature, int offset)
    {
        if (bytes.Length < offset + signature.Length)
        {
            return false;
        }

        for (var i = 0; i < signature.Length; i++)
        {
            if (bytes[offset + i] != signature[i])
            {
                return false;
            }
        }

        return true;
    }

    [GeneratedRegex(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\.(jpg|jpeg|png|webp|avif)$")]
    private static partial Regex GeneratedFileNameRegex();
}
