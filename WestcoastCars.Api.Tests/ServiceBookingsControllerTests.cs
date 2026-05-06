using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;
using WestcoastCars.Application.Features.ServiceBookings.Queries.ListAll;
using WestcoastCars.Contracts.DTOs;
using Xunit;

namespace WestcoastCars.Api.Tests;

public class ServiceBookingsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<ServiceBookingsController>> _loggerMock;
    private readonly ServiceBookingsController _controller;

    public ServiceBookingsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<ServiceBookingsController>>();
        _controller = new ServiceBookingsController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ListAll_ShouldReturnOkAndListOfServiceBookings()
    {
        // Arrange
        var bookings = new List<ServiceBookingSummaryDto>
        {
            new() { Id = 1, VehicleRegistrationNumber = "ABC123" }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListServiceBookingsQuery>(), default)).ReturnsAsync(bookings);

        // Act
        var result = await _controller.ListAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<ServiceBookingSummaryDto>>(okResult.Value);
        Assert.Single(returnValue);
    }

    [Fact]
    public async Task Create_ShouldMapDtoToCommandAndReturnOkWithId()
    {
        // Arrange
        var dto = new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "ABC123",
            ServiceType = "Oil",
            BookingDate = new DateTime(2026, 5, 6),
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            CustomerPhone = "123456",
            Description = "Test booking"
        };

        _mediatorMock.Setup(m => m.Send(It.Is<CreateServiceBookingCommand>(command =>
            command.VehicleRegistrationNumber == dto.VehicleRegistrationNumber &&
            command.ServiceType == dto.ServiceType &&
            command.BookingDate == dto.BookingDate &&
            command.CustomerName == dto.CustomerName &&
            command.CustomerEmail == dto.CustomerEmail &&
            command.CustomerPhone == dto.CustomerPhone &&
            command.Description == dto.Description), default)).ReturnsAsync(10);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var idProperty = okResult.Value?.GetType().GetProperty("id");
        Assert.Equal(10, idProperty?.GetValue(okResult.Value));
    }
}
