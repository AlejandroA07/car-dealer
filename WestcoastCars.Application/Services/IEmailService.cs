using WestcoastCars.Domain.Common.Enums;

namespace WestcoastCars.Application.Services;

public interface IEmailService
{
    Task SendBookingConfirmationAsync(
        string toEmail,
        string customerName,
        DateTime bookingDate,
        TimeSlot timeSlot,
        string serviceType,
        string vehicleRegistrationNumber);

    Task SendCancellationNoticeAsync(
        string toEmail,
        string customerName,
        DateTime bookingDate,
        TimeSlot timeSlot,
        string reason);
}
