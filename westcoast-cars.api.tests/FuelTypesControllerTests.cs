
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Application.Features.FuelTypes.Commands.Create;
using WestcoastCars.Application.Features.FuelTypes.Commands.Delete;
using WestcoastCars.Application.Features.FuelTypes.Commands.Update;
using WestcoastCars.Application.Features.FuelTypes.Queries.GetById;
using WestcoastCars.Application.Features.FuelTypes.Queries.ListAll;
using WestcoastCars.Contracts.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using WestcoastCars.Application.Exceptions;

namespace westcoast_cars.api.tests
{
    public class FuelTypesControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly FuelTypesController _controller;

        public FuelTypesControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new FuelTypesController(_mediatorMock.Object);
        }

        [Fact]
        public async Task ListAll_ShouldReturnOkResult_WithListOfFuelTypes()
        {
            // Arrange
            var fuelTypes = new List<NamedObjectDto>
            {
                new NamedObjectDto { Id = 1, Name = "Gasoline" },
                new NamedObjectDto { Id = 2, Name = "Diesel" }
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllFuelTypesQuery>(), default)).ReturnsAsync(fuelTypes);

            // Act
            var result = await _controller.ListAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<NamedObjectDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetById_ShouldReturnOkResult_WhenFuelTypeExists()
        {
            // Arrange
            var fuelType = new NamedObjectDto { Id = 1, Name = "Gasoline" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetFuelTypeByIdQuery>(), default)).ReturnsAsync(fuelType);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<NamedObjectDto>(okResult.Value);
            Assert.Equal("Gasoline", returnValue.Name);
        }

        [Fact]
        public async Task Add_ShouldReturnCreatedAtActionResult_WhenModelIsValid()
        {
            // Arrange
            var newFuelTypeDto = new NamedObjectDto { Name = "Electric" };
            var returnedFuelTypeDto = new NamedObjectDto { Id = 1, Name = "Electric" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFuelTypeCommand>(), default)).ReturnsAsync(returnedFuelTypeDto);

            // Act
            var result = await _controller.Add(newFuelTypeDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetById", createdAtActionResult.ActionName);
            var returnValue = Assert.IsType<NamedObjectDto>(createdAtActionResult.Value);
            Assert.Equal(newFuelTypeDto.Name, returnValue.Name);
        }

        [Fact]
        public async Task Update_ShouldReturnNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            int fuelTypeId = 1;
            var fuelTypeDto = new NamedObjectDto { Id = fuelTypeId, Name = "UpdatedName" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateFuelTypeCommand>(), default)).Returns(Task.FromResult(Unit.Value));

            // Act
            var result = await _controller.Update(fuelTypeId, fuelTypeDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenFuelTypeExists()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteFuelTypeCommand>(), default)).Returns(Task.FromResult(Unit.Value));

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
