using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Common.Enums;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Delete;

public class DeleteServiceBookingCommandHandler : IRequestHandler<DeleteServiceBookingCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteServiceBookingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteServiceBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.ServiceBookingRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException($"Service booking {request.Id} not found.");

        if (booking.Status != BookingStatus.Cancelled && booking.Status != BookingStatus.Completed)
            throw new ConflictException("Endast inaktiva servicebokningar kan raderas.");

        _unitOfWork.ServiceBookingRepository.Remove(booking);
        await _unitOfWork.CompleteAsync();

        return Unit.Value;
    }
}
