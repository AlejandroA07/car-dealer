
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Application.Features.Transmissions.Commands.Create;
using WestcoastCars.Application.Features.Transmissions.Commands.Delete;
using WestcoastCars.Application.Features.Transmissions.Commands.Update;
using WestcoastCars.Application.Features.Transmissions.Queries.GetById;
using WestcoastCars.Application.Features.Transmissions.Queries.ListAll;
using WestcoastCars.Contracts.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using WestcoastCars.Application.Exceptions;

namespace westcoast_cars.api.tests
{
    public class TransmissionsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly TransmissionsController _controller;

        public TransmissionsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new TransmissionsController(_mediatorMock.Object);
        }

        [Fact]
        public async Task ListAll_ShouldReturnOkResult_WithListOfTransmissionTypes()
        {
            // Arrange
            var transmissionTypes = new List<NamedObjectDto>
            {
                new NamedObjectDto { Id = 1, Name = "Manual" },
                new NamedObjectDto { Id = 2, Name = "Automatic" }
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllTransmissionsQuery>(), default)).ReturnsAsync(transmissionTypes);

            // Act
            var result = await _controller.ListAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<NamedObjectDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetById_ShouldReturnOkResult_WhenTransmissionTypeExists()
        {
            // Arrange
            var transmissionType = new NamedObjectDto { Id = 1, Name = "Manual" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetTransmissionByIdQuery>(), default)).ReturnsAsync(transmissionType);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<NamedObjectDto>(okResult.Value);
            Assert.Equal("Manual", returnValue.Name);
        }

        [Fact]
        public async Task Add_ShouldReturnCreatedAtActionResult_WhenModelIsValid()
        {
            // Arrange
            var newTransmissionTypeDto = new NamedObjectDto { Name = "CVT" };
            var returnedTransmissionTypeDto = new NamedObjectDto { Id = 1, Name = "CVT" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateTransmissionCommand>(), default)).ReturnsAsync(returnedTransmissionTypeDto);

            // Act
            var result = await _controller.Add(newTransmissionTypeDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetById", createdAtActionResult.ActionName);
            var returnValue = Assert.IsType<NamedObjectDto>(createdAtActionResult.Value);
            Assert.Equal(newTransmissionTypeDto.Name, returnValue.Name);
        }

        [Fact]
        public async Task Update_ShouldReturnNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            int transmissionTypeId = 1;
            var transmissionTypeDto = new NamedObjectDto { Id = transmissionTypeId, Name = "UpdatedName" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateTransmissionCommand>(), default)).Returns(Task.FromResult(Unit.Value));

            // Act
            var result = await _controller.Update(transmissionTypeId, transmissionTypeDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenTransmissionTypeExists()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteTransmissionCommand>(), default)).Returns(Task.FromResult(Unit.Value));

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}

