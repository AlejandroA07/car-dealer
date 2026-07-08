using Microsoft.Extensions.Logging;
using WestcoastCars.Application.Services;
using WestcoastCars.Domain.Common.Enums;

namespace WestcoastCars.Infrastructure.Services;

/// <summary>
/// Development-only stand-in for <see cref="IEmailService"/>, used when no SMTP host is
/// configured. Logs the content that would have been emailed (including confirmation links
/// and verification codes) instead of sending it, so the register/confirm and guest-booking
/// OTP flows can be completed locally without any email provider setup.
/// </summary>
public class ConsoleEmailService(ILogger<ConsoleEmailService> logger) : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger = logger;

    public Task SendBookingConfirmationAsync(
        string toEmail,
        string customerName,
        DateTime bookingDate,
        TimeSlot timeSlot,
        string serviceType,
        string vehicleRegistrationNumber)
    {
        _logger.LogWarning(
            "DEV EMAIL (no SMTP configured) — booking confirmation for {Email}: {ServiceType} on {BookingDate} ({TimeSlot}) for {RegistrationNumber}",
            toEmail, serviceType, bookingDate, timeSlot, vehicleRegistrationNumber);
        return Task.CompletedTask;
    }

    public Task SendCancellationNoticeAsync(
        string toEmail,
        string customerName,
        DateTime bookingDate,
        TimeSlot timeSlot,
        string reason)
    {
        _logger.LogWarning(
            "DEV EMAIL (no SMTP configured) — cancellation notice for {Email}: {BookingDate} ({TimeSlot}), reason: {Reason}",
            toEmail, bookingDate, timeSlot, reason);
        return Task.CompletedTask;
    }

    public Task SendEmailVerificationAsync(string toEmail, string name, string confirmationLink)
    {
        _logger.LogWarning(
            "DEV EMAIL (no SMTP configured) — confirmation link for {Email}: {ConfirmationLink}",
            toEmail, confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendVerificationCodeAsync(string toEmail, string code, int expiryMinutes)
    {
        _logger.LogWarning(
            "DEV EMAIL (no SMTP configured) — verification code for {Email}: {Code} (valid {ExpiryMinutes} min)",
            toEmail, code, expiryMinutes);
        return Task.CompletedTask;
    }
}
