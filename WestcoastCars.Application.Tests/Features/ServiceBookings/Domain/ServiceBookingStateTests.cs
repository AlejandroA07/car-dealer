using WestcoastCars.Domain.Common.Enums;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.ServiceBookings.Domain;

public class ServiceBookingStateTests
{
    private static ServiceBooking CreateBooking() => new()
    {
        VehicleRegistrationNumber = "ABC123",
        ServiceType = "Oil change",
        BookingDate = DateTime.UtcNow.AddDays(7),
        CustomerName = "Test Customer",
        CustomerEmail = "test@example.com",
        CustomerPhone = "0700000000"
    };

    [Fact]
    public void Confirm_ShouldTransitionFromPendingToConfirmed()
    {
        var booking = CreateBooking();
        booking.Confirm();
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenAlreadyConfirmed()
    {
        var booking = CreateBooking();
        booking.Confirm();
        Assert.Throws<InvalidOperationException>(() => booking.Confirm());
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenCancelled()
    {
        var booking = CreateBooking();
        booking.Cancel();
        Assert.Throws<InvalidOperationException>(() => booking.Confirm());
    }

    [Fact]
    public void Confirm_ShouldThrow_WhenCompleted()
    {
        var booking = CreateBooking();
        booking.Confirm();
        booking.Complete();
        Assert.Throws<InvalidOperationException>(() => booking.Confirm());
    }

    [Fact]
    public void Cancel_ShouldTransitionFromPendingToCancelled()
    {
        var booking = CreateBooking();
        booking.Cancel();
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void Cancel_ShouldTransitionFromConfirmedToCancelled()
    {
        var booking = CreateBooking();
        booking.Confirm();
        booking.Cancel();
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void Cancel_ShouldThrow_WhenAlreadyCompleted()
    {
        var booking = CreateBooking();
        booking.Confirm();
        booking.Complete();
        Assert.Throws<InvalidOperationException>(() => booking.Cancel());
    }

    [Fact]
    public void Complete_ShouldTransitionFromConfirmedToCompleted()
    {
        var booking = CreateBooking();
        booking.Confirm();
        booking.Complete();
        Assert.Equal(BookingStatus.Completed, booking.Status);
    }

    [Fact]
    public void Complete_ShouldThrow_WhenPending()
    {
        var booking = CreateBooking();
        Assert.Throws<InvalidOperationException>(() => booking.Complete());
    }

    [Fact]
    public void Complete_ShouldThrow_WhenCancelled()
    {
        var booking = CreateBooking();
        booking.Cancel();
        Assert.Throws<InvalidOperationException>(() => booking.Complete());
    }
}
