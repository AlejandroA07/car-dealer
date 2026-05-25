using System.Net;

namespace WestcoastCars.Web.Services;

public class ServiceBookingDataResult<T>
{
    public bool Succeeded { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public string? ErrorMessage { get; init; }
    public T Data { get; init; } = default!;

    public static ServiceBookingDataResult<T> Success(T data) => new()
    {
        Succeeded = true,
        Data = data
    };

    public static ServiceBookingDataResult<T> Failure(HttpStatusCode? statusCode, string errorMessage, T fallbackData) => new()
    {
        StatusCode = statusCode,
        ErrorMessage = errorMessage,
        Data = fallbackData,
        Succeeded = false
    };
}
