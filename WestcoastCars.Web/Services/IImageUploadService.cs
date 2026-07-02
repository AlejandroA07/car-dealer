namespace WestcoastCars.Web.Services;

public interface IImageUploadService
{
    Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken);

    Task DeleteIfOwnedAsync(string? imageUrl, CancellationToken cancellationToken);
}
