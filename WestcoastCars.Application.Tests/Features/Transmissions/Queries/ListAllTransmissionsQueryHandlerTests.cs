using AutoMapper;
using Moq;
using WestcoastCars.Application.Features.Transmissions.Queries.ListAll;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Transmissions.Queries;

public class ListAllTransmissionsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITransmissionTypeRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly ListAllTransmissionsQueryHandler _handler;

    public ListAllTransmissionsQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.TransmissionTypeRepository).Returns(_repositoryMock.Object);
        _handler = new ListAllTransmissionsQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedDtos()
    {
        var transmissions = new List<TransmissionType> { new() { Id = 1, Name = "Manual" } };
        var dtos = new List<NamedObjectDto> { new() { Id = 1, Name = "Manual" } };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(transmissions);
        _mapperMock.Setup(m => m.Map<IEnumerable<NamedObjectDto>>(transmissions)).Returns(dtos);

        var result = await _handler.Handle(new ListAllTransmissionsQuery(), CancellationToken.None);

        Assert.Same(dtos, result);
    }
}
