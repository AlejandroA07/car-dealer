using WestcoastCars.Application.Common.Interfaces.Services;

namespace WestcoastCars.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime LocalNow => DateTime.Now;
}
