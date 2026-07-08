using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Api.Observability;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Cancel;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Complete;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Delete;
using WestcoastCars.Application.Features.ServiceBookings.Queries.GetWeekSlots;
using WestcoastCars.Application.Features.ServiceBookings.Queries.ListAll;
using WestcoastCars.Application.Services;
using WestcoastCars.Contracts.DTOs;
using Xunit;

namespace WestcoastCars.Api.Tests;

public class ServiceBookingsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<ServiceBookingsController>> _loggerMock;
    private readonly Mock<IGuestEmailVerificationService> _guestEmailVerificationServiceMock;
    private readonly AppTelemetry _telemetry;
    private readonly ServiceBookingsController _controller;

    public ServiceBookingsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<ServiceBookingsController>>();
        _guestEmailVerificationServiceMock = new Mock<IGuestEmailVerificationService>();
        _telemetry = new AppTelemetry();
        _controller = new ServiceBookingsController(_mediatorMock.Object, _loggerMock.Object, _telemetry, _guestEmailVerificationServiceMock.Object);
    }

    [Fact]
    public async Task ListAll_ShouldReturnOkWithPagedResult()
    {
        var paged = new PagedResult<ServiceBookingSummaryDto>
        {
            Items = [new() { Id = 1, VehicleRegistrationNumber = "ABC123" }],
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListServiceBookingsQuery>(), default)).ReturnsAsync(paged);

        var result = await _controller.ListAll(new PagedQueryDto(), "active");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PagedResult<ServiceBookingSummaryDto>>(okResult.Value);
        Assert.Single(returnValue.Items);
        Assert.Equal(1, returnValue.TotalCount);
        _mediatorMock.Verify(m => m.Send(It.Is<ListServiceBookingsQuery>(q => q.IsActive == true), default), Times.Once);
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
            TimeSlot = 2,
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            CustomerPhone = "123456",
            Description = "Test booking",
            VerifiedEmailToken = "verified-token"
        };

        _mediatorMock.Setup(m => m.Send(It.Is<CreateServiceBookingCommand>(command =>
            command.VehicleRegistrationNumber == dto.VehicleRegistrationNumber &&
            command.ServiceType == dto.ServiceType &&
            command.BookingDate == dto.BookingDate &&
            (int)command.TimeSlot == dto.TimeSlot &&
            command.CustomerName == dto.CustomerName &&
            command.CustomerEmail == dto.CustomerEmail &&
            command.CustomerPhone == dto.CustomerPhone &&
            command.Description == dto.Description), default)).ReturnsAsync(10);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        var response = Assert.IsType<CreateServiceBookingResponseDto>(createdResult.Value);
        Assert.Equal(10, response.Id);
        _loggerMock.VerifyLog(LogLevel.Information, "Creating new service booking for vehicle", Times.Once());
        _guestEmailVerificationServiceMock.Verify(
            s => s.EnsureEmailIsVerifiedAsync(dto.VerifiedEmailToken, dto.CustomerEmail, default), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldPropagateException_AndSkipBooking_WhenAnonymousVerificationFails()
    {
        var dto = new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "ABC123",
            ServiceType = "Oil",
            BookingDate = new DateTime(2026, 5, 6),
            TimeSlot = 2,
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            CustomerPhone = "123456",
            Description = "Test booking"
        };

        _guestEmailVerificationServiceMock
            .Setup(s => s.EnsureEmailIsVerifiedAsync(dto.VerifiedEmailToken, dto.CustomerEmail, default))
            .ThrowsAsync(new ValidationException("verifiedEmailToken", ["The email address must be verified before booking."]));

        await Assert.ThrowsAsync<ValidationException>(() => _controller.Create(dto));

        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateServiceBookingCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task Create_ShouldSkipVerification_WhenCallerIsAuthenticated()
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([], "TestAuthType"))
            }
        };

        var dto = new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "ABC123",
            ServiceType = "Oil",
            BookingDate = new DateTime(2026, 5, 6),
            TimeSlot = 2,
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            CustomerPhone = "123456",
            Description = "Test booking",
            VerifiedEmailToken = null
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateServiceBookingCommand>(), default)).ReturnsAsync(11);

        var result = await _controller.Create(dto);

        Assert.IsType<CreatedResult>(result);
        _guestEmailVerificationServiceMock.Verify(
            s => s.EnsureEmailIsVerifiedAsync(It.IsAny<string?>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Cancel_ShouldReturnNoContent()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CancelServiceBookingCommand>(), default)).ReturnsAsync(Unit.Value);

        var result = await _controller.Cancel(1, new CancelServiceBookingDto { CancellationReason = "Test reason" });

        Assert.IsType<NoContentResult>(result);
        _mediatorMock.Verify(m => m.Send(It.Is<CancelServiceBookingCommand>(c => c.Id == 1 && c.CancellationReason == "Test reason"), default), Times.Once);
    }

    [Fact]
    public async Task GetAvailability_ShouldReturnOkWithSlots()
    {
        var slots = new[]
        {
            new SlotAvailabilityDto { Date = new DateOnly(2026, 05, 25), TimeSlot = 0, IsBooked = false }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetWeekSlotsQuery>(), default)).ReturnsAsync(slots);

        var result = await _controller.GetAvailability(new DateOnly(2026, 05, 25));

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(slots, okResult.Value);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteServiceBookingCommand>(), default)).ReturnsAsync(Unit.Value);

        var result = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
        _mediatorMock.Verify(m => m.Send(It.Is<DeleteServiceBookingCommand>(c => c.Id == 1), default), Times.Once);
    }

    [Fact]
    public async Task Complete_ShouldReturnNoContent()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CompleteServiceBookingCommand>(), default)).ReturnsAsync(Unit.Value);

        var result = await _controller.Complete(1);

        Assert.IsType<NoContentResult>(result);
        _mediatorMock.Verify(m => m.Send(It.Is<CompleteServiceBookingCommand>(c => c.Id == 1), default), Times.Once);
    }

    [Fact]
    public async Task Create_ShouldPropagateException_WhenMediatorFails()
    {
        var dto = new ServiceBookingPostDto
        {
            VehicleRegistrationNumber = "ABC123",
            ServiceType = "Oil",
            BookingDate = new DateTime(2026, 5, 6),
            TimeSlot = 1,
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            CustomerPhone = "123456",
            Description = "Test booking",
            VerifiedEmailToken = "verified-token"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateServiceBookingCommand>(), default))
            .ThrowsAsync(new InvalidOperationException("Boom"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(dto));
    }
}
