
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> CompleteAsync();
}

