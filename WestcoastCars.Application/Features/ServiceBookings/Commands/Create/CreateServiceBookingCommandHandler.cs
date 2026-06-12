using MediatR;
using Microsoft.Extensions.Logging;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Services;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Create;

public class CreateServiceBookingCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService, ILogger<CreateServiceBookingCommandHandler> logger) : IRequestHandler<CreateServiceBookingCommand, int>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<CreateServiceBookingCommandHandler> _logger = logger;

    public async Task<int> Handle(CreateServiceBookingCommand request, CancellationToken cancellationToken)
    {
        var normalizedRegistrationNumber = request.VehicleRegistrationNumber.Trim().ToUpperInvariant();
        var date = DateOnly.FromDateTime(request.BookingDate);
        ServiceBooking? booking = null;

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existingId = await _unitOfWork.ServiceBookingRepository.FindByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);
            if (existingId.HasValue)
                return existingId.Value;
        }

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var isTaken = await _unitOfWork.ServiceBookingRepository.IsSlotTakenAsync(date, request.TimeSlot);
            if (isTaken)
                throw new ConflictException("Det valda tidsfönstret är redan bokat. Välj ett annat.");

            var hasActiveBooking = await _unitOfWork.ServiceBookingRepository.HasActiveBookingForRegistrationAsync(normalizedRegistrationNumber);
            if (hasActiveBooking)
                throw new ConflictException("Det finns redan en aktiv bokning för detta registreringsnummer.");

            var vehicle = await _unitOfWork.VehicleRepository.FindByRegistrationNumberAsync(normalizedRegistrationNumber);
            booking = new ServiceBooking
            {
                VehicleId = vehicle?.Id,
                VehicleRegistrationNumber = normalizedRegistrationNumber,
                ServiceType = request.ServiceType.Trim(),
                BookingDate = DateTime.SpecifyKind(request.BookingDate.Date, DateTimeKind.Utc),
                TimeSlot = request.TimeSlot,
                CustomerName = request.CustomerName.Trim(),
                CustomerEmail = request.CustomerEmail.Trim(),
                CustomerPhone = request.CustomerPhone.Trim(),
                Description = request.Description.Trim(),
                IdempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey) ? null : request.IdempotencyKey
            };
            booking.Confirm();

            await _unitOfWork.ServiceBookingRepository.AddAsync(booking);
            await _unitOfWork.CompleteOrThrowAsync("Failed to create service booking");
        }, cancellationToken);

        try
        {
            await _emailService.SendBookingConfirmationAsync(
                booking!.CustomerEmail,
                booking.CustomerName,
                booking.BookingDate,
                booking.TimeSlot,
                booking.ServiceType,
                booking.VehicleRegistrationNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Booking {Id} saved but confirmation email failed", booking!.Id);
        }

        return booking!.Id;
    }
}
