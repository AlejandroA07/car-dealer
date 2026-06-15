using AutoMapper;
using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Transmissions.Commands.Create;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Transmissions.Commands;

public class CreateTransmissionCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITransmissionTypeRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly CreateTransmissionCommandHandler _handler;

    public CreateTransmissionCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.TransmissionTypeRepository).Returns(_repositoryMock.Object);
        _handler = new CreateTransmissionCommandHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateTransmission_AndReturnDto()
    {
        var dto = new NamedObjectDto { Id = 1, Name = "Automatic" };

        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<NamedObjectDto>(It.IsAny<TransmissionType>())).Returns(dto);

        var result = await _handler.Handle(
            new CreateTransmissionCommand { Name = "Automatic" }, CancellationToken.None);

        Assert.Equal(dto, result);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<TransmissionType>(t => t.Name == "Automatic")), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowPersistenceException_WhenSaveFails()
    {
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

        await Assert.ThrowsAsync<PersistenceException>(() =>
            _handler.Handle(new CreateTransmissionCommand { Name = "Automatic" }, CancellationToken.None));
    }
}
