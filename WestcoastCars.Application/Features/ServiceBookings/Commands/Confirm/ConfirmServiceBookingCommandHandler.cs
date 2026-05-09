using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Confirm;

public class ConfirmServiceBookingCommandHandler : IRequestHandler<ConfirmServiceBookingCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmServiceBookingCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ConfirmServiceBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _unitOfWork.ServiceBookingRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException($"Service booking {request.Id} not found.");

        try
        {
            booking.Confirm();
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _unitOfWork.CompleteAsync();
        return Unit.Value;
    }
}
