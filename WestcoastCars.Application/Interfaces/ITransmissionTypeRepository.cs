
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface ITransmissionTypeRepository : IRepository<TransmissionType>
{
    Task<TransmissionType?> FindByIdWithVehiclesAsync(int id);
    Task<IReadOnlyList<TransmissionType>> FindByNameWithVehiclesAsync(string name);
}

