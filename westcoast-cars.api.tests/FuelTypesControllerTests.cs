
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using westcoast_cars.api.Controllers;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Api.Exceptions;

namespace westcoast_cars.api.tests
{
    public class FuelTypesControllerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<FuelTypesController>> _loggerMock;
        private readonly FuelTypesController _controller;

        public FuelTypesControllerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<FuelTypesController>>();
            _controller = new FuelTypesController(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ListAll_ShouldReturnOkResult_WithListOfFuelTypes()
        {
            // Arrange
            var fuelTypes = new List<FuelType>
            {
                new FuelType { Id = 1, Name = "Gasoline" },
                new FuelType { Id = 2, Name = "Diesel" }
            };
            var repositoryMock = new Mock<IRepository<FuelType>>();
            repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(fuelTypes);
            _unitOfWorkMock.Setup(uow => uow.Repository<FuelType>()).Returns(repositoryMock.Object);

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
            var fuelType = new FuelType { Id = 1, Name = "Gasoline" };
            var repositoryMock = new Mock<IRepository<FuelType>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(fuelType);
            _unitOfWorkMock.Setup(uow => uow.Repository<FuelType>()).Returns(repositoryMock.Object);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<FuelType>(okResult.Value);
            Assert.Equal("Gasoline", returnValue.Name);
        }

        [Fact]
        public async Task GetById_ShouldThrowNotFoundException_WhenFuelTypeDoesNotExist()
        {
            // Arrange
            var repositoryMock = new Mock<IRepository<FuelType>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((FuelType)null);
            _unitOfWorkMock.Setup(uow => uow.Repository<FuelType>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetById(1));
        }

        [Fact]
        public async Task Add_ShouldReturnCreatedAtActionResult_WhenModelIsValid()
        {
            // Arrange
            var newFuelTypeDto = new NamedObjectDto { Name = "Electric" };
            var repositoryMock = new Mock<IRepository<FuelType>>();
            repositoryMock.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<FuelType, bool>>>())).ReturnsAsync((FuelType)null);
            repositoryMock.Setup(repo => repo.AddAsync(It.IsAny<FuelType>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.Repository<FuelType>()).Returns(repositoryMock.Object);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.Add(newFuelTypeDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetById", createdAtActionResult.ActionName);
        }

        [Fact]
        public async Task Add_ShouldThrowConflictException_WhenFuelTypeExists()
        {
            // Arrange
            var existingFuelTypeDto = new NamedObjectDto { Name = "Gasoline" };
            var existingFuelType = new FuelType { Id = 1, Name = "Gasoline" };
            var repositoryMock = new Mock<IRepository<FuelType>>();
            repositoryMock.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<FuelType, bool>>>())).ReturnsAsync(existingFuelType);
            _unitOfWorkMock.Setup(uow => uow.Repository<FuelType>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _controller.Add(existingFuelTypeDto));
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenFuelTypeExists()
        {
            // Arrange
            var fuelTypeToDelete = new FuelType { Id = 1, Name = "Gasoline" };
            var repositoryMock = new Mock<IRepository<FuelType>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(fuelTypeToDelete);
            _unitOfWorkMock.Setup(uow => uow.Repository<FuelType>()).Returns(repositoryMock.Object);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldThrowNotFoundException_WhenFuelTypeDoesNotExist()
        {
            // Arrange
            var repositoryMock = new Mock<IRepository<FuelType>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((FuelType)null);
            _unitOfWorkMock.Setup(uow => uow.Repository<FuelType>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _controller.Delete(1));
        }
    }
}
