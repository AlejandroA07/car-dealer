using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Domain.Common.Enums;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Create;

public class CreateServiceBookingCommandHandler : IRequestHandler<CreateServiceBookingCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceBookingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateServiceBookingCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.VehicleRepository.FindByRegistrationNumberAsync(request.VehicleRegistrationNumber);
        var booking = new ServiceBooking
        {
            VehicleId = vehicle?.Id,
            VehicleRegistrationNumber = request.VehicleRegistrationNumber,
            ServiceType = request.ServiceType,
            BookingDate = request.BookingDate,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            Description = request.Description,
            Status = BookingStatus.Pending
        };

        await _unitOfWork.ServiceBookingRepository.AddAsync(booking);

        await _unitOfWork.CompleteOrThrowAsync("Failed to create service booking");
        return booking.Id;
    }
}
