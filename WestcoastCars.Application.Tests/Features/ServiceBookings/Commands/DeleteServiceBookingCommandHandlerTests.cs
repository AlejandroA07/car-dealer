using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Delete;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Common.Enums;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.ServiceBookings.Commands;

public class DeleteServiceBookingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IServiceBookingRepository> _repositoryMock = new();

    public DeleteServiceBookingCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ServiceBookingRepository).Returns(_repositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_ShouldDeleteInactiveBooking()
    {
        var booking = CreateBooking(1);
        booking.Confirm();
        booking.Cancel();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        var handler = new DeleteServiceBookingCommandHandler(_unitOfWorkMock.Object);

        await handler.Handle(new DeleteServiceBookingCommand { Id = 1 }, CancellationToken.None);

        _repositoryMock.Verify(r => r.Remove(booking), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenBookingIsActive()
    {
        var booking = CreateBooking(1);
        booking.Confirm();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        var handler = new DeleteServiceBookingCommandHandler(_unitOfWorkMock.Object);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new DeleteServiceBookingCommand { Id = 1 }, CancellationToken.None));
    }

    private static ServiceBooking CreateBooking(int id) => new()
    {
        Id = id,
        VehicleRegistrationNumber = "ABC123",
        ServiceType = "Oil change",
        BookingDate = new DateTime(2026, 05, 24),
        TimeSlot = TimeSlot.Morning,
        CustomerName = "Test Customer",
        CustomerEmail = "test@example.com",
        CustomerPhone = "0700000000"
    };
}
