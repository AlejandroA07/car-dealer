using WestcoastCars.Auth.Application.Common.Interfaces.Services;

namespace WestcoastCars.Auth.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}