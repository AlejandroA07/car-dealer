using Moq;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.ServiceBookings.Commands;

public class CreateServiceBookingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock = new();
    private readonly Mock<IServiceBookingRepository> _serviceBookingRepositoryMock = new();

    public CreateServiceBookingCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.VehicleRepository).Returns(_vehicleRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.ServiceBookingRepository).Returns(_serviceBookingRepositoryMock.Object);
        _unitOfWorkMock.Setup(unitOfWork => unitOfWork.CompleteAsync()).ReturnsAsync(1);
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

        var handler = new CreateServiceBookingCommandHandler(_unitOfWorkMock.Object);

        await handler.Handle(CreateCommand("ABC123"), CancellationToken.None);

        Assert.NotNull(capturedBooking);
        Assert.Equal(42, capturedBooking!.VehicleId);
        Assert.Equal("ABC123", capturedBooking.VehicleRegistrationNumber);
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

        var handler = new CreateServiceBookingCommandHandler(_unitOfWorkMock.Object);

        await handler.Handle(CreateCommand("UNKNOWN1"), CancellationToken.None);

        Assert.NotNull(capturedBooking);
        Assert.Null(capturedBooking!.VehicleId);
        Assert.Equal("UNKNOWN1", capturedBooking.VehicleRegistrationNumber);
    }

    private static CreateServiceBookingCommand CreateCommand(string registrationNumber) =>
        new()
        {
            VehicleRegistrationNumber = registrationNumber,
            ServiceType = "Annual service",
            BookingDate = DateTime.UtcNow.AddDays(1),
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
