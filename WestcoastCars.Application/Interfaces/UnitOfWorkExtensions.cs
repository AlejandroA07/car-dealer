using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Interfaces;

public static class UnitOfWorkExtensions
{
    public static async Task CompleteOrThrowAsync(this IUnitOfWork unitOfWork, string failureMessage)
    {
        if (await unitOfWork.CompleteAsync() == 0)
        {
            throw new PersistenceException(failureMessage);
        }
    }
}
