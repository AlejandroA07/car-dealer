
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Application.Features.Vehicles.Commands.Create;
using WestcoastCars.Application.Features.Vehicles.Commands.Delete;
using WestcoastCars.Application.Features.Vehicles.Commands.MarkAsSold;
using WestcoastCars.Application.Features.Vehicles.Commands.Update;
using WestcoastCars.Application.Features.Vehicles.Queries.GetById;
using WestcoastCars.Application.Features.Vehicles.Queries.GetByRegNo;
using WestcoastCars.Application.Features.Vehicles.Queries.ListAll;
using WestcoastCars.Contracts.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace westcoast_cars.api.tests;

public class VehiclesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<VehiclesController>> _loggerMock;
    private readonly VehiclesController _controller;

    public VehiclesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<VehiclesController>>();
        _controller = new VehiclesController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ListAll_ShouldReturnOkAndListOfVehicles()
    {
        // Arrange
        var vehicles = new List<VehicleSummaryDto>
        {
            new VehicleSummaryDto { Id = 1, Name = "Volvo V60" }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllVehiclesQuery>(), default)).ReturnsAsync(vehicles);

        // Act
        var result = await _controller.ListAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<VehicleSummaryDto>>(okResult.Value);
        Assert.Single(returnValue);
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
    }

    [Fact]
    public async Task Add_ShouldCreateVehicleAndReturnCreatedAtAction()
    {
        // Arrange
        var command = new CreateVehicleCommand { RegistrationNumber = "NEWCAR1" };
        var vehicle = new VehicleDetailsDto { Id = 1, RegistrationNumber = "NEWCAR1" };
        
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateVehicleCommand>(), default)).ReturnsAsync(1);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetVehicleByIdQuery>(), default)).ReturnsAsync(vehicle);

        // Act
        var result = await _controller.Add(command);

        // Assert
        var createdAtAction = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("GetById", createdAtAction.ActionName);
        var returnValue = Assert.IsType<VehicleDetailsDto>(createdAtAction.Value);
        Assert.Equal(1, returnValue.Id);
    }

    [Fact]
    public async Task UpdateVehicle_ShouldReturnNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        var command = new UpdateVehicleCommand { Id = 1, RegistrationNumber = "UPDATED" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateVehicleCommand>(), default)).ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.UpdateVehicle(1, command);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task MarkAsSold_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<MarkAsSoldCommand>(), default)).ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.MarkAsSold(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteVehicleCommand>(), default)).ReturnsAsync(Unit.Value);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}
