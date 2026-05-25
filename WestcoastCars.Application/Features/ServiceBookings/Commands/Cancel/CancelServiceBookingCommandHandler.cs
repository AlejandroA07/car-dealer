using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Services;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Cancel;

public class CancelServiceBookingCommandHandler : IRequestHandler<CancelServiceBookingCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public CancelServiceBookingCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
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

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _unitOfWork.CompleteOrThrowAsync("Failed to cancel service booking");

            await _emailService.SendCancellationNoticeAsync(
                booking.CustomerEmail,
                booking.CustomerName,
                booking.BookingDate,
                booking.TimeSlot,
                request.CancellationReason.Trim());
        }, cancellationToken);

        return Unit.Value;
    }
}
