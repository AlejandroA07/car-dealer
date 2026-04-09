using WestcoastCars.Application.Models.Blocket;

namespace WestcoastCars.Application.Interfaces;

public interface IBlocketVehicleImportMapper
{
    BlocketVehicleImportData Map(BlocketCarSearchItem searchItem, BlocketCarAdDetails? adDetails, DateTime importedAtUtc);
}
