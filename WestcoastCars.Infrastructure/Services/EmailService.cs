using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Services;
using WestcoastCars.Domain.Common.Enums;
using WestcoastCars.Infrastructure.Options;

namespace WestcoastCars.Infrastructure.Services;

public class EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailOptions _options = options.Value;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task SendBookingConfirmationAsync(
        string toEmail,
        string customerName,
        DateTime bookingDate,
        TimeSlot timeSlot,
        string serviceType,
        string vehicleRegistrationNumber)
    {
        var subject = "Bokningsbekräftelse – Westcoast Cars";
        var body = $"""
            Hej {customerName},

            Din servicebokning är bekräftad!

            Detaljer:
              Datum:         {bookingDate:yyyy-MM-dd}
              Tid:           {SlotWindow(timeSlot)}
              Typ av service: {serviceType}
              Registrering:  {vehicleRegistrationNumber}

            Vi ser fram emot ditt besök. Kontakta oss om du har frågor.

            Vänliga hälsningar,
            Westcoast Cars
            """;

        await SendAsync(toEmail, customerName, subject, body);
    }

    public async Task SendCancellationNoticeAsync(
        string toEmail,
        string customerName,
        DateTime bookingDate,
        TimeSlot timeSlot,
        string reason)
    {
        var subject = "Avbokad service – Westcoast Cars";
        var body = $"""
            Hej {customerName},

            Vi behöver tyvärr avboka din servicetid.

            Avbokad tid:
              Datum: {bookingDate:yyyy-MM-dd}
              Tid:   {SlotWindow(timeSlot)}

            Meddelande från oss:
            {reason}

            Kontakta oss gärna om du vill boka en ny tid.

            Vänliga hälsningar,
            Westcoast Cars
            """;

        await SendAsync(toEmail, customerName, subject, body);
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost) || string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            _logger.LogError("Email not configured. Subject: {Subject}", subject);
            throw new PersistenceException("E-post för servicebokningar är inte konfigurerad.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.StartTls);

            if (!string.IsNullOrWhiteSpace(_options.Username))
                await client.AuthenticateAsync(_options.Username, _options.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent: \"{Subject}\"", subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email. Subject: \"{Subject}\"", subject);
            throw new PersistenceException("Det gick inte att skicka e-post för servicebokningen.");
        }
    }

    private static string SlotWindow(TimeSlot slot) => slot switch
    {
        TimeSlot.Morning => "08:00 – 10:00",
        TimeSlot.MidMorning => "10:00 – 12:00",
        TimeSlot.Afternoon => "13:00 – 15:00",
        _ => slot.ToString()
    };
}
