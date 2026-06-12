using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Services;
using WestcoastCars.Domain.Common.Enums;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.ServiceBookings.Commands;

public class CreateServiceBookingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock = new();
    private readonly Mock<IServiceBookingRepository> _serviceBookingRepositoryMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();

    public CreateServiceBookingCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.VehicleRepository).Returns(_vehicleRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.ServiceBookingRepository).Returns(_serviceBookingRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.CompleteAsync()).ReturnsAsync(1);
        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>((action, _) => action());
        _serviceBookingRepositoryMock
            .Setup(r => r.IsSlotTakenAsync(It.IsAny<DateOnly>(), It.IsAny<TimeSlot>()))
            .ReturnsAsync(false);
        _serviceBookingRepositoryMock
            .Setup(r => r.HasActiveBookingForRegistrationAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
    }

    [Fact]
    public async Task Handle_ShouldLinkBookingToVehicle_WhenRegistrationExists()
    {
        var vehicle = CreateVehicle(42, "ABC123");
        ServiceBooking? capturedBooking = null;
        _vehicleRepositoryMock
            .Setup(repository => repository.FindByRegistrationNumberAsync("ABC123"))
            .ReturnsAsync(vehicle);
        _serviceBookingRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<ServiceBooking>()))
            .Callback<ServiceBooking>(booking => capturedBooking = booking)
            .Returns(Task.CompletedTask);

        var handler = new CreateServiceBookingCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object, NullLogger<CreateServiceBookingCommandHandler>.Instance);

        await handler.Handle(CreateCommand("ABC123"), CancellationToken.None);

        Assert.NotNull(capturedBooking);
        Assert.Equal(42, capturedBooking!.VehicleId);
        Assert.Equal("ABC123", capturedBooking.VehicleRegistrationNumber);
        _emailServiceMock.Verify(e => e.SendBookingConfirmationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(),
            It.IsAny<TimeSlot>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAllowBookingWithoutVehicleLink_WhenRegistrationDoesNotExist()
    {
        ServiceBooking? capturedBooking = null;
        _vehicleRepositoryMock
            .Setup(repository => repository.FindByRegistrationNumberAsync("UNKNOWN1"))
            .ReturnsAsync((Vehicle?)null);
        _serviceBookingRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<ServiceBooking>()))
            .Callback<ServiceBooking>(booking => capturedBooking = booking)
            .Returns(Task.CompletedTask);

        var handler = new CreateServiceBookingCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object, NullLogger<CreateServiceBookingCommandHandler>.Instance);

        await handler.Handle(CreateCommand("UNKNOWN1"), CancellationToken.None);

        Assert.NotNull(capturedBooking);
        Assert.Null(capturedBooking!.VehicleId);
        Assert.Equal("UNKNOWN1", capturedBooking.VehicleRegistrationNumber);
        _emailServiceMock.Verify(e => e.SendBookingConfirmationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(),
            It.IsAny<TimeSlot>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenRegistrationAlreadyHasActiveBooking()
    {
        _serviceBookingRepositoryMock
            .Setup(r => r.HasActiveBookingForRegistrationAsync("ABC123"))
            .ReturnsAsync(true);

        var handler = new CreateServiceBookingCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object, NullLogger<CreateServiceBookingCommandHandler>.Instance);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(CreateCommand("ABC123"), CancellationToken.None));

        _serviceBookingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ServiceBooking>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendBookingConfirmationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(),
            It.IsAny<TimeSlot>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflictException_WhenSlotIsAlreadyTaken()
    {
        _serviceBookingRepositoryMock
            .Setup(r => r.IsSlotTakenAsync(It.IsAny<DateOnly>(), It.IsAny<TimeSlot>()))
            .ReturnsAsync(true);

        var handler = new CreateServiceBookingCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object, NullLogger<CreateServiceBookingCommandHandler>.Instance);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(CreateCommand("ABC123"), CancellationToken.None));

        _serviceBookingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ServiceBooking>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendBookingConfirmationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(),
            It.IsAny<TimeSlot>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSaveBookingAndNotThrow_WhenEmailSendingFails()
    {
        _serviceBookingRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<ServiceBooking>()))
            .Returns(Task.CompletedTask);
        _emailServiceMock
            .Setup(e => e.SendBookingConfirmationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<TimeSlot>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new PersistenceException("Email failed"));

        var handler = new CreateServiceBookingCommandHandler(_unitOfWorkMock.Object, _emailServiceMock.Object, NullLogger<CreateServiceBookingCommandHandler>.Instance);

        await handler.Handle(CreateCommand("ABC123"), CancellationToken.None);

        _serviceBookingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<ServiceBooking>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendBookingConfirmationAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(),
            It.IsAny<TimeSlot>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    private static CreateServiceBookingCommand CreateCommand(string registrationNumber) =>
        new()
        {
            VehicleRegistrationNumber = registrationNumber,
            ServiceType = "Annual service",
            BookingDate = DateTime.UtcNow.AddDays(1),
            TimeSlot = TimeSlot.Afternoon,
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            CustomerPhone = "0700000000",
            Description = "Test booking"
        };

    private static Vehicle CreateVehicle(int id, string registrationNumber) =>
        new()
        {
            Id = id,
            RegistrationNumber = registrationNumber,
            Model = "V60",
            ModelYear = 2024,
            ImageUrl = "/images/no-car.png",
            Description = "Test vehicle",
            Manufacturer = new Manufacturer { Name = "VOLVO" },
            FuelType = new FuelType { Name = "Petrol" },
            TransmissionType = new TransmissionType { Name = "Automatic" }
        };
}
