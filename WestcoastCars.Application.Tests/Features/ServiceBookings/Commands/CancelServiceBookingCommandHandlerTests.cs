using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Cancel;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Services;
using WestcoastCars.Domain.Common.Enums;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.ServiceBookings.Commands;

public class CancelServiceBookingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IServiceBookingRepository> _repositoryMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();

    public CancelServiceBookingCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ServiceBookingRepository).Returns(_repositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>((action, _) => action());
    }

    [Fact]
    public async Task Handle_ShouldCancelBooking_WhenPending()
    {
        var booking = CreatePendingBooking(1);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        var handler = new CancelServiceBookingCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object);
        await handler.Handle(new CancelServiceBookingCommand { Id = 1, CancellationReason = "Tekniker sjuk" }, CancellationToken.None);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
        _emailServiceMock.Verify(e => e.SendCancellationNoticeAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(),
            It.IsAny<TimeSlot>(), "Tekniker sjuk"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCancelBooking_WhenConfirmed()
    {
        var booking = CreatePendingBooking(1);
        booking.Confirm();
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        var handler = new CancelServiceBookingCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object);
        await handler.Handle(new CancelServiceBookingCommand { Id = 1, CancellationReason = "Tekniker sjuk" }, CancellationToken.None);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        _emailServiceMock.Verify(e => e.SendCancellationNoticeAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(),
            It.IsAny<TimeSlot>(), "Tekniker sjuk"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenBookingDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ServiceBooking?)null);

        var handler = new CancelServiceBookingCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object);

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

        var handler = new CancelServiceBookingCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CancelServiceBookingCommand { Id = 1 }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenEmailSendingFails()
    {
        var booking = CreatePendingBooking(1);
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _emailServiceMock
            .Setup(e => e.SendCancellationNoticeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<TimeSlot>(),
                It.IsAny<string>()))
            .ThrowsAsync(new PersistenceException("Email failed"));

        var handler = new CancelServiceBookingCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object);

        await Assert.ThrowsAsync<PersistenceException>(() =>
            handler.Handle(new CancelServiceBookingCommand { Id = 1, CancellationReason = "Tekniskt fel" }, CancellationToken.None));
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
