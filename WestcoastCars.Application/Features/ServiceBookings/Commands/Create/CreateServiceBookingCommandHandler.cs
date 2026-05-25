using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Services;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Create;

public class CreateServiceBookingCommandHandler : IRequestHandler<CreateServiceBookingCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public CreateServiceBookingCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<int> Handle(CreateServiceBookingCommand request, CancellationToken cancellationToken)
    {
        var normalizedRegistrationNumber = request.VehicleRegistrationNumber.Trim().ToUpperInvariant();
        var date = DateOnly.FromDateTime(request.BookingDate);
        ServiceBooking? booking = null;

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
                BookingDate = request.BookingDate.Date,
                TimeSlot = request.TimeSlot,
                CustomerName = request.CustomerName.Trim(),
                CustomerEmail = request.CustomerEmail.Trim(),
                CustomerPhone = request.CustomerPhone.Trim(),
                Description = request.Description.Trim()
            };
            booking.Confirm();

            await _unitOfWork.ServiceBookingRepository.AddAsync(booking);
            await _unitOfWork.CompleteOrThrowAsync("Failed to create service booking");

            await _emailService.SendBookingConfirmationAsync(
                booking.CustomerEmail,
                booking.CustomerName,
                booking.BookingDate,
                booking.TimeSlot,
                booking.ServiceType,
                booking.VehicleRegistrationNumber);
        }, cancellationToken);

        return booking!.Id;
    }
}
