using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Web.Controllers;
using WestcoastCars.Web.Services;
using WestcoastCars.Web.ViewModels.CoreAttributes;
using WestcoastCars.Web.ViewModels.Vehicles;

namespace WestcoastCars.Api.Tests;

public class WebVehiclesControllerTests
{
    private readonly Mock<IVehicleService> _vehicleServiceMock = new();
    private readonly Mock<IManufacturerService> _manufacturerServiceMock = new();

    public WebVehiclesControllerTests()
    {
        _manufacturerServiceMock
            .Setup(s => s.ListAllAsync())
            .ReturnsAsync([]);
    }

    [Fact]
    public async Task Index_NoFilters_ShouldCallListVehiclesAsync_AndReturnView()
    {
        _vehicleServiceMock
            .Setup(s => s.ListVehiclesAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new PagedResult<VehicleSummaryDto>());

        var controller = CreateController();

        var result = await controller.Index(new VehicleSearchDto(), 1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        _vehicleServiceMock.Verify(s => s.ListVehiclesAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _vehicleServiceMock.Verify(s => s.SearchVehiclesAsync(It.IsAny<VehicleSearchDto>()), Times.Never);
    }

    [Fact]
    public async Task Index_WithFilters_ShouldCallSearchVehiclesAsync_AndReturnView()
    {
        _vehicleServiceMock
            .Setup(s => s.SearchVehiclesAsync(It.IsAny<VehicleSearchDto>()))
            .ReturnsAsync(new PagedResult<VehicleSummaryDto>());

        var controller = CreateController();
        var search = new VehicleSearchDto { Make = "Volvo" };

        var result = await controller.Index(search, 1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        _vehicleServiceMock.Verify(s => s.SearchVehiclesAsync(It.IsAny<VehicleSearchDto>()), Times.Once);
        _vehicleServiceMock.Verify(s => s.ListVehiclesAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Details_ShouldReturnView_WhenVehicleFound()
    {
        var dto = new VehicleDetailsDto { Id = 1 };
        _vehicleServiceMock.Setup(s => s.GetVehicleByIdAsync(1)).ReturnsAsync(dto);

        var controller = CreateController();

        var result = await controller.Details(1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Details", view.ViewName);
        Assert.Same(dto, view.Model);
    }

    [Fact]
    public async Task Details_ShouldReturnNotFound_WhenVehicleIsNull()
    {
        _vehicleServiceMock.Setup(s => s.GetVehicleByIdAsync(99)).ReturnsAsync((VehicleDetailsDto?)null);

        var controller = CreateController();

        var result = await controller.Details(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task HanteraDatabas_ShouldReturnView_WithModel()
    {
        var viewModel = new HanteraDatabaseViewModel();
        _vehicleServiceMock.Setup(s => s.GetHanteraDatabaseViewModelAsync()).ReturnsAsync(viewModel);

        var controller = CreateController();

        var result = await controller.HanteraDatabas();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("HanteraDatabas", view.ViewName);
        Assert.Same(viewModel, view.Model);
    }

    [Fact]
    public async Task BulkDelete_ShouldRedirectToHanteraDatabas_WithSuccessTempData()
    {
        _vehicleServiceMock
            .Setup(s => s.BulkDeleteAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<int?>(), It.IsAny<int?>()))
            .ReturnsAsync(3);

        var controller = CreateController();

        var result = await controller.BulkDelete(null, null, null, null, null);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(VehiclesController.HanteraDatabas), redirect.ActionName);
        Assert.Contains("3", controller.TempData["success"]?.ToString());
    }

    [Fact]
    public async Task DeleteAll_ShouldRedirectToHanteraDatabas_WithSuccessTempData()
    {
        _vehicleServiceMock.Setup(s => s.DeleteAllVehiclesAsync()).ReturnsAsync(5);

        var controller = CreateController();

        var result = await controller.DeleteAll();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(VehiclesController.HanteraDatabas), redirect.ActionName);
        Assert.Contains("5", controller.TempData["success"]?.ToString());
    }

    [Fact]
    public async Task PreviewBlocket_ValidModel_ShouldSetPreviewResults_AndReturnView()
    {
        var previews = new List<BlocketPreviewDto> { new() { ExternalListingId = "X1" } };
        _vehicleServiceMock
            .Setup(s => s.PreviewBlocketAsync(It.IsAny<BlocketSyncViewModel>()))
            .ReturnsAsync(previews);

        var controller = CreateController();
        var model = new BlocketSyncViewModel();

        var result = await controller.PreviewBlocket(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("SyncBlocket", view.ViewName);
        var returned = Assert.IsType<BlocketSyncViewModel>(view.Model);
        Assert.True(returned.HasPreview);
        Assert.Equal(previews, returned.PreviewResults);
    }

    [Fact]
    public async Task PreviewBlocket_InvalidModelState_ShouldReturnSyncBlocketView_WithoutCallingService()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("field", "error");
        var model = new BlocketSyncViewModel();

        var result = await controller.PreviewBlocket(model);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("SyncBlocket", view.ViewName);
        _vehicleServiceMock.Verify(s => s.PreviewBlocketAsync(It.IsAny<BlocketSyncViewModel>()), Times.Never);
    }

    [Fact]
    public async Task ImportSelected_ShouldReturnSyncBlocketView_WithImportResult()
    {
        var importResult = new ImportSelectedResult { TotalAdded = 2 };
        _vehicleServiceMock
            .Setup(s => s.ImportSelectedAsync(It.IsAny<List<string>>(), It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(importResult);

        var controller = CreateController();

        var result = await controller.ImportSelected(["EXT-1", "EXT-2"], []);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("SyncBlocket", view.ViewName);
        var returned = Assert.IsType<BlocketSyncViewModel>(view.Model);
        Assert.Equal(importResult, returned.ImportResult);
    }

    [Fact]
    public async Task MarkAsSold_ShouldRedirectToIndex_WhenSuccessful()
    {
        _vehicleServiceMock.Setup(s => s.MarkAsSoldAsync(1)).ReturnsAsync(true);

        var controller = CreateController();

        var result = await controller.MarkAsSold(1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(VehiclesController.Index), redirect.ActionName);
    }

    [Fact]
    public async Task MarkAsSold_ShouldReturnErrorsView_WhenNotSuccessful()
    {
        _vehicleServiceMock.Setup(s => s.MarkAsSoldAsync(1)).ReturnsAsync(false);

        var controller = CreateController();

        var result = await controller.MarkAsSold(1);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Errors", view.ViewName);
    }

    private VehiclesController CreateController()
    {
        var httpContext = new DefaultHttpContext();
        var controller = new VehiclesController(
            _vehicleServiceMock.Object,
            _manufacturerServiceMock.Object,
            NullLogger<VehiclesController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                httpContext,
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>())
        };
        return controller;
    }
}
