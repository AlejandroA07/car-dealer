
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Application.Features.Manufacturers.Commands.Create;
using WestcoastCars.Application.Features.Manufacturers.Commands.Delete;
using WestcoastCars.Application.Features.Manufacturers.Commands.Update;
using WestcoastCars.Application.Features.Manufacturers.Queries.GetById;
using WestcoastCars.Application.Features.Manufacturers.Queries.ListAll;
using WestcoastCars.Contracts.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using WestcoastCars.Application.Exceptions;

namespace westcoast_cars.api.tests
{
    public class ManufacturersControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ManufacturersController _controller;

        public ManufacturersControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ManufacturersController(_mediatorMock.Object);
        }

        [Fact]
        public async Task ListAll_ShouldReturnOkResult_WithListOfManufacturers()
        {
            // Arrange
            var manufacturers = new List<NamedObjectDto>
            {
                new NamedObjectDto { Id = 1, Name = "Volvo" },
                new NamedObjectDto { Id = 2, Name = "Saab" }
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllManufacturersQuery>(), default)).ReturnsAsync(manufacturers);

            // Act
            var result = await _controller.ListAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<NamedObjectDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetById_ShouldReturnOkResult_WhenManufacturerExists()
        {
            // Arrange
            var manufacturer = new NamedObjectDto { Id = 1, Name = "Volvo" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetManufacturerByIdQuery>(), default)).ReturnsAsync(manufacturer);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<NamedObjectDto>(okResult.Value);
            Assert.Equal("Volvo", returnValue.Name);
        }

        [Fact]
        public async Task Add_ShouldReturnCreatedAtActionResult_WhenModelIsValid()
        {
            // Arrange
            var newManufacturerDto = new NamedObjectDto { Name = "Tesla" };
            var returnedManufacturerDto = new NamedObjectDto { Id = 1, Name = "Tesla" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateManufacturerCommand>(), default)).ReturnsAsync(returnedManufacturerDto);

            // Act
            var result = await _controller.Add(newManufacturerDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetById", createdAtActionResult.ActionName);
            var returnValue = Assert.IsType<NamedObjectDto>(createdAtActionResult.Value);
            Assert.Equal(newManufacturerDto.Name, returnValue.Name);
        }

        [Fact]
        public async Task Update_ShouldReturnNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            int manufacturerId = 1;
            var manufacturerDto = new NamedObjectDto { Id = manufacturerId, Name = "UpdatedName" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateManufacturerCommand>(), default)).Returns(Task.FromResult(Unit.Value));

            // Act
            var result = await _controller.Update(manufacturerId, manufacturerDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenManufacturerExists()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteManufacturerCommand>(), default)).Returns(Task.FromResult(Unit.Value));

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}

