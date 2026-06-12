namespace WestcoastCars.Application.Common.Interfaces.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
}
