using WestcoastCars.Application.Models.Blocket;

namespace WestcoastCars.Application.Interfaces;

public interface IBlocketApiClient
{
    Task<BlocketCarSearchResponse> SearchCarsAsync(BlocketCarSearchRequest request, CancellationToken cancellationToken = default);
    Task<BlocketCarAdDetails> GetCarAdAsync(string id, CancellationToken cancellationToken = default);
}
