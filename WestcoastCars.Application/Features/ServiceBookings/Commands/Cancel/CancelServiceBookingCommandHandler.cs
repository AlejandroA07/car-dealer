using MediatR;
using Microsoft.Extensions.Logging;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Services;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Cancel;

public class CancelServiceBookingCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService, ILogger<CancelServiceBookingCommandHandler> logger) : IRequestHandler<CancelServiceBookingCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<CancelServiceBookingCommandHandler> _logger = logger;

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

        await _unitOfWork.CompleteOrThrowAsync("Failed to cancel service booking");

        try
        {
            await _emailService.SendCancellationNoticeAsync(
                booking.CustomerEmail,
                booking.CustomerName,
                booking.BookingDate,
                booking.TimeSlot,
                request.CancellationReason.Trim());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Booking {Id} cancelled but cancellation email failed", booking.Id);
        }

        return Unit.Value;
    }
}
