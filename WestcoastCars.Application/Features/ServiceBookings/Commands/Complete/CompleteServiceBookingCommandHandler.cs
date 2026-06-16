using MediatR;
using WestcoastCars.Application.Common.Interfaces.Services;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Complete;

public class CompleteServiceBookingCommandHandler(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider) : IRequestHandler<CompleteServiceBookingCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Unit> Handle(CompleteServiceBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.ServiceBookingRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException($"Service booking {request.Id} not found.");

        if (booking.BookingDate.Date > _dateTimeProvider.LocalNow.Date)
            throw new ConflictException("Det går inte att markera en service som klar före bokningsdatumet.");

        try
        {
            booking.Complete();
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _unitOfWork.CompleteOrThrowAsync("Failed to complete service booking");
        return Unit.Value;
    }
}
