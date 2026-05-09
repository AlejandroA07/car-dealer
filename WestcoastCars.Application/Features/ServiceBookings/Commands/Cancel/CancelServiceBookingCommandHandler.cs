using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Cancel;

public class CancelServiceBookingCommandHandler : IRequestHandler<CancelServiceBookingCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelServiceBookingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CancelServiceBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.ServiceBookingRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException($"Service booking {request.Id} not found.");

        try
        {
            booking.Cancel();
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _unitOfWork.CompleteAsync();
        return Unit.Value;
    }
}
