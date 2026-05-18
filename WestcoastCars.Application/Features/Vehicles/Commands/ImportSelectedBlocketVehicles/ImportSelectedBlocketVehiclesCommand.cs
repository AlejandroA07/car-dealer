using MediatR;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Commands.ImportSelectedBlocketVehicles;

public class ImportSelectedBlocketVehiclesCommand : IRequest<ImportSelectedResult>
{
    public List<string> ExternalListingIds { get; set; } = [];
    public Dictionary<string, string> ImageUrlsById { get; set; } = [];
}
