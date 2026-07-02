using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using WestcoastCars.Web.Configurations;
using WestcoastCars.Web.Services;
using Xunit;

namespace WestcoastCars.Web.Tests.Services;

public class LocalImageUploadServiceTests : IDisposable
{
    private readonly string _storagePath;
    private readonly LocalImageUploadService _sut;

    private static readonly byte[] JpegBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01];
    private static readonly byte[] PngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];

    public LocalImageUploadServiceTests()
    {
        _storagePath = Path.Combine(Path.GetTempPath(), "westcoast-image-upload-tests", Guid.NewGuid().ToString());
        var options = Options.Create(new ImageUploadOptions
        {
            StoragePath = _storagePath,
            UrlPath = "/images/uploads",
            MaxFileSizeBytes = 1024
        });
        _sut = new LocalImageUploadService(options);
    }

    public void Dispose()
    {
        if (Directory.Exists(_storagePath))
        {
            Directory.Delete(_storagePath, recursive: true);
        }
    }

    private static IFormFile CreateFormFile(byte[] content, string fileName = "photo.jpg", string contentType = "image/jpeg")
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public async Task SaveAsync_ValidJpeg_WritesFileAndReturnsUrl()
    {
        var file = CreateFormFile(JpegBytes);

        var url = await _sut.SaveAsync(file, CancellationToken.None);

        Assert.StartsWith("/images/uploads/", url);
        Assert.EndsWith(".jpg", url);
        var savedFileName = url["/images/uploads/".Length..];
        Assert.True(File.Exists(Path.Combine(_storagePath, savedFileName)));
    }

    [Fact]
    public async Task SaveAsync_ValidPng_WritesFileWithPngExtension()
    {
        var file = CreateFormFile(PngBytes, "photo.png", "image/png");

        var url = await _sut.SaveAsync(file, CancellationToken.None);

        Assert.EndsWith(".png", url);
    }

    [Fact]
    public async Task SaveAsync_SpoofedContentType_IsRejectedByMagicBytes()
    {
        // Plain text bytes, but claims to be a JPEG via ContentType and filename.
        var textBytes = "not actually an image"u8.ToArray();
        var file = CreateFormFile(textBytes, "fake.jpg", "image/jpeg");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SaveAsync(file, CancellationToken.None));
        Assert.False(Directory.Exists(_storagePath) && Directory.GetFiles(_storagePath).Length > 0);
    }

    [Fact]
    public async Task SaveAsync_OversizedFile_IsRejected()
    {
        var oversized = new byte[2048];
        Array.Copy(JpegBytes, oversized, JpegBytes.Length);
        var file = CreateFormFile(oversized);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SaveAsync(file, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteIfOwnedAsync_OwnedFile_IsDeleted()
    {
        var file = CreateFormFile(JpegBytes);
        var url = await _sut.SaveAsync(file, CancellationToken.None);
        var savedFileName = url["/images/uploads/".Length..];
        var fullPath = Path.Combine(_storagePath, savedFileName);
        Assert.True(File.Exists(fullPath));

        await _sut.DeleteIfOwnedAsync(url, CancellationToken.None);

        Assert.False(File.Exists(fullPath));
    }

    [Theory]
    [InlineData("https://example.com/foo.jpg")]
    [InlineData("/images/no-car.png")]
    [InlineData("/images/uploads/../../etc/passwd")]
    [InlineData(null)]
    public async Task DeleteIfOwnedAsync_NotOwned_DoesNothing(string? imageUrl)
    {
        Directory.CreateDirectory(_storagePath);
        var sentinelPath = Path.Combine(_storagePath, "sentinel.txt");
        await File.WriteAllTextAsync(sentinelPath, "keep-me");

        await _sut.DeleteIfOwnedAsync(imageUrl, CancellationToken.None);

        Assert.True(File.Exists(sentinelPath));
    }
}
