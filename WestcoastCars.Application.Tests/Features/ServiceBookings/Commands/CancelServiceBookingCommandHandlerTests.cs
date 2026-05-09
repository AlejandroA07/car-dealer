using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Cancel;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Common.Enums;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.ServiceBookings.Commands;

public class CancelServiceBookingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IServiceBookingRepository> _repositoryMock = new();

    public CancelServiceBookingCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ServiceBookingRepository).Returns(_repositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_ShouldCancelBooking_WhenPending()
    {
        var booking = CreatePendingBooking(1);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        var handler = new CancelServiceBookingCommandHandler(_unitOfWorkMock.Object);
        await handler.Handle(new CancelServiceBookingCommand { Id = 1 }, CancellationToken.None);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancelBooking_WhenConfirmed()
    {
        var booking = CreatePendingBooking(1);
        booking.Confirm();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        var handler = new CancelServiceBookingCommandHandler(_unitOfWorkMock.Object);
        await handler.Handle(new CancelServiceBookingCommand { Id = 1 }, CancellationToken.None);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenBookingDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ServiceBooking?)null);

        var handler = new CancelServiceBookingCommandHandler(_unitOfWorkMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CancelServiceBookingCommand { Id = 99 }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenCompleted()
    {
        var booking = CreatePendingBooking(1);
        booking.Confirm();
        booking.Complete();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        var handler = new CancelServiceBookingCommandHandler(_unitOfWorkMock.Object);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CancelServiceBookingCommand { Id = 1 }, CancellationToken.None));
    }

    private static ServiceBooking CreatePendingBooking(int id) => new()
    {
        Id = id,
        VehicleRegistrationNumber = "ABC123",
        ServiceType = "Oil change",
        BookingDate = DateTime.UtcNow.AddDays(7),
        CustomerName = "Test Customer",
        CustomerEmail = "test@example.com",
        CustomerPhone = "0700000000"
    };
}
