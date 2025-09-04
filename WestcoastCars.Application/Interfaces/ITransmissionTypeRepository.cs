using System.Linq.Expressions;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface ITransmissionTypeRepository
{
    Task<TransmissionType?> FindByIdAsync(int id);
    Task<IReadOnlyList<TransmissionType>> ListAllAsync();
    Task<IReadOnlyList<TransmissionType>> ListAsync(Expression<Func<TransmissionType, bool>> expression);
    Task<TransmissionType?> FindByIdWithVehiclesAsync(int id);
    Task<IReadOnlyList<TransmissionType>> FindByNameWithVehiclesAsync(string name);
    void Add(TransmissionType transmissionType);
    void Delete(int id);
}
