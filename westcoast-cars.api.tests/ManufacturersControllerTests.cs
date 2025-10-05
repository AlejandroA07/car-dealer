
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WestcoastCars.Api.Controllers;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Api.Exceptions;

namespace westcoast_cars.api.tests
{
    public class ManufacturersControllerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<ManufacturersController>> _loggerMock;
        private readonly ManufacturersController _controller;

        public ManufacturersControllerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<ManufacturersController>>();
            _controller = new ManufacturersController(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ListAll_ShouldReturnOkResult_WithListOfManufacturers()
        {
            // Arrange
            var manufacturers = new List<Manufacturer>
            {
                new Manufacturer { Id = 1, Name = "Volvo" },
                new Manufacturer { Id = 2, Name = "Saab" }
            };
            var repositoryMock = new Mock<IRepository<Manufacturer>>();
            repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(manufacturers);
            _unitOfWorkMock.Setup(uow => uow.Repository<Manufacturer>()).Returns(repositoryMock.Object);

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
            var manufacturer = new Manufacturer { Id = 1, Name = "Volvo" };
            var repositoryMock = new Mock<IRepository<Manufacturer>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(manufacturer);
            _unitOfWorkMock.Setup(uow => uow.Repository<Manufacturer>()).Returns(repositoryMock.Object);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<NamedObjectDto>(okResult.Value);
            Assert.Equal("Volvo", returnValue.Name);
        }

        [Fact]
        public async Task GetById_ShouldThrowNotFoundException_WhenManufacturerDoesNotExist()
        {
            // Arrange
            var repositoryMock = new Mock<IRepository<Manufacturer>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Manufacturer)null);
            _unitOfWorkMock.Setup(uow => uow.Repository<Manufacturer>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetById(1));
        }

        [Fact]
        public async Task Add_ShouldReturnCreatedAtActionResult_WhenModelIsValid()
        {
            // Arrange
            var newManufacturerDto = new NamedObjectDto { Name = "Tesla" };
            var repositoryMock = new Mock<IRepository<Manufacturer>>();
            repositoryMock.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Manufacturer, bool>>>())).ReturnsAsync((Manufacturer)null);
            repositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Manufacturer>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.Repository<Manufacturer>()).Returns(repositoryMock.Object);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.Add(newManufacturerDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetById", createdAtActionResult.ActionName);
            var returnValue = Assert.IsType<NamedObjectDto>(createdAtActionResult.Value);
            Assert.Equal(newManufacturerDto.Name, returnValue.Name);
        }

        [Fact]
        public async Task Add_ShouldReturnBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.Add(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Request body cannot be null.", badRequestResult.Value);
        }

        [Fact]
        public async Task Add_ShouldThrowConflictException_WhenManufacturerExists()
        {
            // Arrange
            var existingManufacturerDto = new NamedObjectDto { Name = "Volvo" };
            var existingManufacturer = new Manufacturer { Id = 1, Name = "Volvo" };
            var repositoryMock = new Mock<IRepository<Manufacturer>>();
            repositoryMock.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Manufacturer, bool>>>())).ReturnsAsync(existingManufacturer);
            _unitOfWorkMock.Setup(uow => uow.Repository<Manufacturer>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _controller.Add(existingManufacturerDto));
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenManufacturerExists()
        {
            // Arrange
            var manufacturerToDelete = new Manufacturer { Id = 1, Name = "Volvo" };
            var repositoryMock = new Mock<IRepository<Manufacturer>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(manufacturerToDelete);
            _unitOfWorkMock.Setup(uow => uow.Repository<Manufacturer>()).Returns(repositoryMock.Object);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldThrowNotFoundException_WhenManufacturerDoesNotExist()
        {
            // Arrange
            var repositoryMock = new Mock<IRepository<Manufacturer>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Manufacturer)null);
            _unitOfWorkMock.Setup(uow => uow.Repository<Manufacturer>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _controller.Delete(1));
        }

        [Fact]
        public async Task Update_ShouldReturnNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            int manufacturerId = 1;
            var manufacturerDto = new NamedObjectDto { Id = manufacturerId, Name = "UpdatedName" };
            var existingManufacturer = new Manufacturer { Id = manufacturerId, Name = "OriginalName" };

            var repositoryMock = new Mock<IRepository<Manufacturer>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(manufacturerId)).ReturnsAsync(existingManufacturer);
            repositoryMock.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Manufacturer, bool>>>())).ReturnsAsync((Manufacturer)null);
            _unitOfWorkMock.Setup(uow => uow.Repository<Manufacturer>()).Returns(repositoryMock.Object);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.Update(manufacturerId, manufacturerDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            repositoryMock.Verify(repo => repo.Update(It.Is<Manufacturer>(m => m.Id == manufacturerId && m.Name == "UpdatedName")), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_WhenModelIsNull()
        {
            // Act
            var result = await _controller.Update(1, null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Request body cannot be null.", badRequestResult.Value);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenManufacturerDoesNotExist()
        {
            // Arrange
            int manufacturerId = 1;
            var manufacturerDto = new NamedObjectDto { Id = manufacturerId, Name = "UpdatedName" };

            var repositoryMock = new Mock<IRepository<Manufacturer>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(manufacturerId)).ReturnsAsync((Manufacturer)null);
            _unitOfWorkMock.Setup(uow => uow.Repository<Manufacturer>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _controller.Update(manufacturerId, manufacturerDto));
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_WhenIdMismatch()
        {
            // Arrange
            int urlId = 1;
            var manufacturerDto = new NamedObjectDto { Id = 2, Name = "UpdatedName" };

            // Act
            var result = await _controller.Update(urlId, manufacturerDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID mismatch", badRequestResult.Value);
        }

        [Fact]
        public async Task Update_ShouldThrowConflictException_WhenNameAlreadyExists()
        {
            // Arrange
            int manufacturerId = 1;
            var manufacturerDto = new NamedObjectDto { Id = manufacturerId, Name = "ExistingName" };
            var existingManufacturer = new Manufacturer { Id = manufacturerId, Name = "OriginalName" };
            var conflictingManufacturer = new Manufacturer { Id = 2, Name = "ExistingName" };

            var repositoryMock = new Mock<IRepository<Manufacturer>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(manufacturerId)).ReturnsAsync(existingManufacturer);
            repositoryMock.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Manufacturer, bool>>>())).ReturnsAsync(conflictingManufacturer);
            _unitOfWorkMock.Setup(uow => uow.Repository<Manufacturer>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _controller.Update(manufacturerId, manufacturerDto));
        }
    }
}
