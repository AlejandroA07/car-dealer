
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
    public class TransmissionsControllerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<TransmissionsController>> _loggerMock;
        private readonly TransmissionsController _controller;

        public TransmissionsControllerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<TransmissionsController>>();
            _controller = new TransmissionsController(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ListAll_ShouldReturnOkResult_WithListOfTransmissionTypes()
        {
            // Arrange
            var transmissionTypes = new List<TransmissionType>
            {
                new TransmissionType { Id = 1, Name = "Manual" },
                new TransmissionType { Id = 2, Name = "Automatic" }
            };
            var repositoryMock = new Mock<IRepository<TransmissionType>>();
            repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(transmissionTypes);
            _unitOfWorkMock.Setup(uow => uow.Repository<TransmissionType>()).Returns(repositoryMock.Object);

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
            var transmissionType = new TransmissionType { Id = 1, Name = "Manual" };
            var repositoryMock = new Mock<IRepository<TransmissionType>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(transmissionType);
            _unitOfWorkMock.Setup(uow => uow.Repository<TransmissionType>()).Returns(repositoryMock.Object);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<TransmissionType>(okResult.Value);
            Assert.Equal("Manual", returnValue.Name);
        }

        [Fact]
        public async Task GetById_ShouldThrowNotFoundException_WhenTransmissionTypeDoesNotExist()
        {
            // Arrange
            var repositoryMock = new Mock<IRepository<TransmissionType>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((TransmissionType)null);
            _unitOfWorkMock.Setup(uow => uow.Repository<TransmissionType>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _controller.GetById(1));
        }

        [Fact]
        public async Task Add_ShouldReturnCreatedAtActionResult_WhenModelIsValid()
        {
            // Arrange
            var newTransmissionTypeDto = new NamedObjectDto { Name = "CVT" };
            var repositoryMock = new Mock<IRepository<TransmissionType>>();
            repositoryMock.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TransmissionType, bool>>>())).ReturnsAsync((TransmissionType)null);
            repositoryMock.Setup(repo => repo.AddAsync(It.IsAny<TransmissionType>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.Repository<TransmissionType>()).Returns(repositoryMock.Object);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.Add(newTransmissionTypeDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetById", createdAtActionResult.ActionName);
        }

        [Fact]
        public async Task Add_ShouldThrowConflictException_WhenTransmissionTypeExists()
        {
            // Arrange
            var existingTransmissionTypeDto = new NamedObjectDto { Name = "Manual" };
            var existingTransmissionType = new TransmissionType { Id = 1, Name = "Manual" };
            var repositoryMock = new Mock<IRepository<TransmissionType>>();
            repositoryMock.Setup(repo => repo.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<TransmissionType, bool>>>())).ReturnsAsync(existingTransmissionType);
            _unitOfWorkMock.Setup(uow => uow.Repository<TransmissionType>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _controller.Add(existingTransmissionTypeDto));
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenTransmissionTypeExists()
        {
            // Arrange
            var transmissionTypeToDelete = new TransmissionType { Id = 1, Name = "Manual" };
            var repositoryMock = new Mock<IRepository<TransmissionType>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(transmissionTypeToDelete);
            _unitOfWorkMock.Setup(uow => uow.Repository<TransmissionType>()).Returns(repositoryMock.Object);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldThrowNotFoundException_WhenTransmissionTypeDoesNotExist()
        {
            // Arrange
            var repositoryMock = new Mock<IRepository<TransmissionType>>();
            repositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((TransmissionType)null);
            _unitOfWorkMock.Setup(uow => uow.Repository<TransmissionType>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _controller.Delete(1));
        }
    }
}
