using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Confirm;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Common.Enums;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.ServiceBookings.Commands;

public class ConfirmServiceBookingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IServiceBookingRepository> _repositoryMock = new();

    public ConfirmServiceBookingCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ServiceBookingRepository).Returns(_repositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_ShouldConfirmBooking_WhenPending()
    {
        var booking = CreatePendingBooking(1);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        var handler = new ConfirmServiceBookingCommandHandler(_unitOfWorkMock.Object);
        await handler.Handle(new ConfirmServiceBookingCommand { Id = 1 }, CancellationToken.None);

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenBookingDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ServiceBooking?)null);

        var handler = new ConfirmServiceBookingCommandHandler(_unitOfWorkMock.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new ConfirmServiceBookingCommand { Id = 99 }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenAlreadyConfirmed()
    {
        var booking = CreatePendingBooking(1);
        booking.Confirm();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        var handler = new ConfirmServiceBookingCommandHandler(_unitOfWorkMock.Object);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new ConfirmServiceBookingCommand { Id = 1 }, CancellationToken.None));
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
