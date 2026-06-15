using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Web.Controllers;
using WestcoastCars.Web.Services;
using WestcoastCars.Web.ViewModels;

namespace WestcoastCars.Api.Tests;

public class WebAdminControllerTests
{
    private readonly Mock<IVehicleService> _vehicleServiceMock = new();

    [Fact]
    public async Task Index_ShouldReturnAdminView_WithComputedStats()
    {
        _vehicleServiceMock.Setup(s => s.ListAllVehiclesAsync()).ReturnsAsync(
        [
            new VehicleSummaryDto { Id = 1, Manufacturer = "Volvo", IsSold = false, Price = 100_000 },
            new VehicleSummaryDto { Id = 2, Manufacturer = "BMW",   IsSold = true,  Price = 200_000 }
        ]);

        var controller = CreateController();

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Admin", view.ViewName);
        var model = Assert.IsType<AdminDashboardViewModel>(view.Model);
        Assert.Equal(2, model.TotalVehicles);
        Assert.Equal(1, model.SoldVehicles);
        Assert.Equal(1, model.AvailableVehicles);
        Assert.Equal(300_000, model.TotalInventoryValue);
    }

    [Fact]
    public async Task Index_ShouldReturnEmptyDashboard_WhenNoVehicles()
    {
        _vehicleServiceMock.Setup(s => s.ListAllVehiclesAsync()).ReturnsAsync([]);

        var controller = CreateController();

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AdminDashboardViewModel>(view.Model);
        Assert.Equal(0, model.TotalVehicles);
        Assert.Empty(model.RecentVehicles);
    }

    private AdminController CreateController()
    {
        var controller = new AdminController(_vehicleServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        return controller;
    }
}
