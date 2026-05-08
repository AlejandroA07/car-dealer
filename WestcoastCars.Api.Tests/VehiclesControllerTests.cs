using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Api.Observability;
using WestcoastCars.Application.Features.Vehicles.Commands.Create;
using WestcoastCars.Application.Features.Vehicles.Commands.Delete;
using WestcoastCars.Application.Features.Vehicles.Commands.MarkAsSold;
using WestcoastCars.Application.Features.Vehicles.Commands.SyncBlocket;
using WestcoastCars.Application.Features.Vehicles.Commands.Update;
using WestcoastCars.Application.Features.Vehicles.Queries.GetById;
using WestcoastCars.Application.Features.Vehicles.Queries.GetByRegNo;
using WestcoastCars.Application.Features.Vehicles.Queries.ListAll;
using WestcoastCars.Application.Features.Vehicles.Queries.Search;
using WestcoastCars.Contracts.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace WestcoastCars.Api.Tests;

public class VehiclesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<VehiclesController>> _loggerMock;
    private readonly AppTelemetry _telemetry;
    private readonly VehiclesController _controller;

    public VehiclesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<VehiclesController>>();
        _telemetry = new AppTelemetry();
        _controller = new VehiclesController(_mediatorMock.Object, _loggerMock.Object, _telemetry);
    }

    [Fact]
    public async Task ListAll_ShouldReturnOkAndListOfVehicles()
    {
        // Arrange
        var vehicles = new PagedResult<VehicleSummaryDto>
        {
            Items = new List<VehicleSummaryDto>
            {
                new VehicleSummaryDto { Id = 1, Name = "Volvo V60" }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllVehiclesQuery>(), default)).ReturnsAsync(vehicles);

        // Act
        var result = await _controller.ListAll(new PagedQueryDto());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PagedResult<VehicleSummaryDto>>(okResult.Value);
        Assert.Single(returnValue.Items);
        _mediatorMock.Verify(m => m.Send(It.Is<ListAllVehiclesQuery>(query => query.Page == 1 && query.PageSize == 20), default), Times.Once);
        _loggerMock.VerifyLog(LogLevel.Information, "Retrieving list of unsold vehicles via MediatR", Times.Once());
    }

    [Fact]
    public async Task GetById_ShouldReturnOkAndVehicle_WhenVehicleExists()
    {
        // Arrange
        var vehicle = new VehicleDetailsDto { Id = 1, RegistrationNumber = "TEST123" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetVehicleByIdQuery>(), default)).ReturnsAsync(vehicle);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<VehicleDetailsDto>(okResult.Value);
        Assert.Equal(1, returnValue.Id);
        _mediatorMock.Verify(m => m.Send(It.Is<GetVehicleByIdQuery>(query => query.Id == 1), default), Times.Once);
    }

    [Fact]
    public async Task GetByRegNo_ShouldReturnOkAndVehicle_WhenVehicleExists()
    {
        // Arrange
        var vehicle = new VehicleDetailsDto { Id = 1, RegistrationNumber = "TEST123" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetVehicleByRegNoQuery>(), default)).ReturnsAsync(vehicle);

        // Act
        var result = await _controller.GetByRegNo("TEST123");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<VehicleDetailsDto>(okResult.Value);
        Assert.Equal("TEST123", returnValue.RegistrationNumber);
        _mediatorMock.Verify(m => m.Send(It.Is<GetVehicleByRegNoQuery>(query => query.RegistrationNumber == "TEST123"), default), Times.Once);
    }

    [Fact]
    public async Task Search_ShouldReturnOkAndForwardSearchCriteria()
    {
        var search = new VehicleSearchDto { Make = "Volvo", Model = "XC60", Page = 2, PageSize = 15 };
        var expectedResult = new PagedResult<VehicleSummaryDto> { Items = [], TotalCount = 0, Page = 2, PageSize = 15 };

        _mediatorMock
            .Setup(m => m.Send(It.Is<SearchVehiclesQuery>(query =>
                query.Criteria.Make == "Volvo" &&
                query.Criteria.Model == "XC60" &&
                query.Criteria.Page == 2 &&
                query.Criteria.PageSize == 15), default))
            .ReturnsAsync(expectedResult);

        var result = await _controller.Search(search);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expectedResult, okResult.Value);
        _loggerMock.VerifyLog(LogLevel.Information, "Searching vehicles with criteria", Times.Once());
    }

    [Fact]
    public async Task Add_ShouldCreateVehicleAndReturnCreatedAtAction()
    {
        // Arrange
        var dto = new VehiclePostDto
        {
            RegistrationNumber = "NEWCAR1",
            ManufacturerId = 1,
            Model = "V60",
            ModelYear = "2024",
            Mileage = 1000,
            FuelTypeId = 2,
            TransmissionTypeId = 3,
            Value = 450000,
            Description = "Test",
            IsSold = false,
            ImageUrl = "test.png"
        };
        var vehicle = new VehicleDetailsDto { Id = 1, RegistrationNumber = "NEWCAR1" };

        _mediatorMock.Setup(m => m.Send(It.Is<CreateVehicleCommand>(command =>
            command.RegistrationNumber == dto.RegistrationNumber &&
            command.ManufacturerId == dto.ManufacturerId &&
            command.Model == dto.Model &&
            command.ModelYear == dto.ModelYear &&
            command.Mileage == dto.Mileage &&
            command.FuelTypeId == dto.FuelTypeId &&
            command.TransmissionTypeId == dto.TransmissionTypeId &&
            command.Value == dto.Value &&
            command.Description == dto.Description &&
            command.IsSold == dto.IsSold &&
            command.ImageUrl == dto.ImageUrl), default)).ReturnsAsync(vehicle);

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetById", createdAtAction.ActionName);
        var returnValue = Assert.IsType<VehicleDetailsDto>(createdAtAction.Value);
        Assert.Equal(1, returnValue.Id);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetVehicleByIdQuery>(), default), Times.Never);
        _loggerMock.VerifyLog(LogLevel.Information, "Creating new vehicle with registration", Times.Once());
    }

    [Fact]
    public async Task UpdateVehicle_ShouldReturnNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        var dto = new VehicleUpdateDto
        {
            Id = 1,
            RegistrationNumber = "UPDATED",
            ManufacturerId = 1,
            Model = "V60",
            ModelYear = "2024",
            Mileage = 1000,
            FuelTypeId = 2,
            TransmissionTypeId = 3,
            Value = 450000,
            Description = "Updated",
            IsSold = true,
            ImageUrl = "updated.png"
        };
        _mediatorMock.Setup(m => m.Send(It.Is<UpdateVehicleCommand>(command =>
            command.Id == dto.Id &&
            command.RegistrationNumber == dto.RegistrationNumber &&
            command.ManufacturerId == dto.ManufacturerId &&
            command.Model == dto.Model &&
            command.ModelYear == dto.ModelYear &&
            command.Mileage == dto.Mileage &&
            command.FuelTypeId == dto.FuelTypeId &&
            command.TransmissionTypeId == dto.TransmissionTypeId &&
            command.Value == dto.Value &&
            command.Description == dto.Description &&
            command.IsSold == dto.IsSold &&
            command.ImageUrl == dto.ImageUrl), default)).ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.UpdateVehicle(1, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mediatorMock.Verify(m => m.Send(It.Is<UpdateVehicleCommand>(command =>
            command.Id == dto.Id &&
            command.RegistrationNumber == dto.RegistrationNumber &&
            command.ImageUrl == dto.ImageUrl), default), Times.Once);
    }

    [Fact]
    public async Task UpdateVehicle_ShouldReturnBadRequest_WhenRouteIdDoesNotMatchDtoId()
    {
        // Arrange
        var dto = new VehicleUpdateDto { Id = 2, RegistrationNumber = "UPDATED" };

        // Act
        var result = await _controller.UpdateVehicle(1, dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateVehicleCommand>(), default), Times.Never);
    }

    [Fact]
    public async Task SyncBlocket_ShouldReturnOkAndSyncSummary()
    {
        var syncResult = new SyncBlocketVehiclesResult
        {
            RequestedLimit = 50,
            AppliedLimit = 50,
            TotalImported = 50,
            TotalReplaced = 12
        };

        _mediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<SyncBlocketVehiclesCommand>(), default))
            .ReturnsAsync(syncResult);

        var result = await _controller.SyncBlocket(new SyncBlocketVehiclesCommand { Limit = 50 });

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<SyncBlocketVehiclesResult>(okResult.Value);
        Assert.Equal(50, returnValue.TotalImported);
        Assert.Equal(12, returnValue.TotalReplaced);
    }

    [Fact]
    public async Task SyncBlocket_ShouldUseDefaultCommand_WhenBodyIsNull()
    {
        _mediatorMock
            .Setup(mediator => mediator.Send(It.IsAny<SyncBlocketVehiclesCommand>(), default))
            .ReturnsAsync(new SyncBlocketVehiclesResult());

        await _controller.SyncBlocket(null);

        _mediatorMock.Verify(mediator => mediator.Send(
            It.Is<SyncBlocketVehiclesCommand>(command => command.Limit == 50),
            default), Times.Once);
    }

    [Fact]
    public async Task MarkAsSold_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.Is<MarkAsSoldCommand>(command => command.Id == 1), default)).ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.MarkAsSold(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mediatorMock.Verify(m => m.Send(It.Is<MarkAsSoldCommand>(command => command.Id == 1), default), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.Is<DeleteVehicleCommand>(command => command.Id == 1), default)).ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mediatorMock.Verify(m => m.Send(It.Is<DeleteVehicleCommand>(command => command.Id == 1), default), Times.Once);
    }
}
