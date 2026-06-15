using Moq;
using WestcoastCars.Application.Features.Vehicles.Queries.Stats;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Queries;

public class GetVehicleStatsByModelQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _repositoryMock = new();
    private readonly GetVehicleStatsByModelQueryHandler _handler;

    public GetVehicleStatsByModelQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_repositoryMock.Object);
        _handler = new GetVehicleStatsByModelQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnStatsDtoFromRepository()
    {
        var stats = new List<VehicleStatsByModelDto> { new("XC60", 5) };
        _repositoryMock.Setup(r => r.GetStatsByModelAsync()).ReturnsAsync(stats);

        var result = await _handler.Handle(new GetVehicleStatsByModelQuery(), CancellationToken.None);

        Assert.Same(stats, result);
    }
}
