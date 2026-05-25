using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WestcoastCars.Web.Controllers;
using WestcoastCars.Web.Services;
using WestcoastCars.Web.ViewModels.ServiceBooking;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Api.Tests;

public class ServiceControllerTests
{
    private readonly Mock<IServiceBookingService> _bookingServiceMock = new();

    [Fact]
    public async Task IndexGet_ShouldMarkAvailabilityFailure_WhenSlotsCannotBeLoaded()
    {
        _bookingServiceMock
            .Setup(service => service.GetWeekSlotsAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(ServiceBookingDataResult<IReadOnlyList<SlotAvailabilityDto>>.Failure(
                null,
                "Det gick inte att hämta lediga tider just nu.",
                []));

        var controller = CreateController();

        var result = await controller.Index(null);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ServiceIndexViewModel>(view.Model);
        Assert.True(model.AvailabilityLoadFailed);
    }

    [Fact]
    public async Task IndexPost_ShouldAddModelError_WhenCreateFails()
    {
        _bookingServiceMock
            .Setup(service => service.CreateBookingAsync(It.IsAny<ServiceBookingViewModel>()))
            .ReturnsAsync(ServiceBookingActionResult.Failure(System.Net.HttpStatusCode.Conflict, "Slot already taken"));
        _bookingServiceMock
            .Setup(service => service.GetWeekSlotsAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync(ServiceBookingDataResult<IReadOnlyList<SlotAvailabilityDto>>.Success([]));

        var controller = CreateController();
        var model = CreateIndexModel();

        var result = await controller.Index(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal("Slot already taken", controller.ModelState[string.Empty]!.Errors.Single().ErrorMessage);
        Assert.Same(model, view.Model);
    }

    [Fact]
    public async Task CompleteBooking_ShouldSetErrorTempData_WhenApiCallFails()
    {
        _bookingServiceMock
            .Setup(service => service.CompleteAsync(4))
            .ReturnsAsync(ServiceBookingActionResult.Failure(System.Net.HttpStatusCode.Conflict, "Cannot complete yet"));

        var controller = CreateController("Admin");

        var result = await controller.CompleteBooking(4);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Cannot complete yet", controller.TempData["error"]);
    }

    [Fact]
    public async Task DeleteBooking_ShouldRedirectToHistory_WhenDeleteSucceeds()
    {
        _bookingServiceMock
            .Setup(service => service.DeleteAsync(8))
            .ReturnsAsync(ServiceBookingActionResult.Success());

        var controller = CreateController("Admin");

        var result = await controller.DeleteBooking(8);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ServiceController.AdminHistory), redirect.ActionName);
    }

    private ServiceController CreateController(params string[] roles)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                roles.Select(role => new Claim(ClaimTypes.Role, role)),
                "TestAuth"))
        };

        var controller = new ServiceController(_bookingServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                httpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>())
        };

        return controller;
    }

    private static ServiceIndexViewModel CreateIndexModel() => new()
    {
        WeekStart = new DateOnly(2026, 05, 25),
        BookingForm = new ServiceBookingViewModel
        {
            VehicleRegistrationNumber = "ABC123",
            ServiceType = "Bas-service",
            BookingDate = new DateTime(2026, 05, 25),
            TimeSlot = 2,
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            CustomerPhone = "0700000000"
        }
    };
}
