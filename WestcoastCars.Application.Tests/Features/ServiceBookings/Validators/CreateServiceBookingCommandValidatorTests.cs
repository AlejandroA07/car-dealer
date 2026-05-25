using Moq;
using WestcoastCars.Application.Common.Interfaces.Services;
using WestcoastCars.Application.Features.ServiceBookings.Commands.Create;
using WestcoastCars.Application.Features.ServiceBookings.Validators;
using WestcoastCars.Domain.Common.Enums;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.ServiceBookings.Validators;

public class CreateServiceBookingCommandValidatorTests
{
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();

    public CreateServiceBookingCommandValidatorTests()
    {
        _dateTimeProviderMock.SetupGet(x => x.LocalNow).Returns(new DateTime(2026, 05, 24, 11, 0, 0));
    }

    [Fact]
    public async Task Validate_ShouldAllowLaterSlotOnSameDay()
    {
        var validator = new CreateServiceBookingCommandValidator(_dateTimeProviderMock.Object);

        var result = await validator.ValidateAsync(CreateCommand(new DateTime(2026, 05, 24), TimeSlot.Afternoon));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldRejectPastSlotOnSameDay()
    {
        var validator = new CreateServiceBookingCommandValidator(_dateTimeProviderMock.Object);

        var result = await validator.ValidateAsync(CreateCommand(new DateTime(2026, 05, 24), TimeSlot.Morning));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Det valda tidsfönstret har redan passerat.");
    }

    private static CreateServiceBookingCommand CreateCommand(DateTime bookingDate, TimeSlot timeSlot) => new()
    {
        VehicleRegistrationNumber = "ABC123",
        ServiceType = "Annual service",
        BookingDate = bookingDate,
        TimeSlot = timeSlot,
        CustomerName = "Test Customer",
        CustomerEmail = "test@example.com",
        CustomerPhone = "0700000000",
        Description = "Test booking"
    };
}
