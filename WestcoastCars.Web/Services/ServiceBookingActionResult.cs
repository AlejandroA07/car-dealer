using System.Net;

namespace WestcoastCars.Web.Services;

public class ServiceBookingActionResult
{
    public bool Succeeded { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public string? ErrorMessage { get; init; }
    public int? BookingId { get; init; }

    public static ServiceBookingActionResult Success(int? bookingId = null) => new()
    {
        Succeeded = true,
        BookingId = bookingId
    };

    public static ServiceBookingActionResult Failure(HttpStatusCode? statusCode, string errorMessage) => new()
    {
        StatusCode = statusCode,
        ErrorMessage = errorMessage,
        Succeeded = false
    };
}
