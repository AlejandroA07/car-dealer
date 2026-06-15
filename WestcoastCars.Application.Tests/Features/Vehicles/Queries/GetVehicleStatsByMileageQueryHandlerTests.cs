using Moq;
using WestcoastCars.Application.Features.Vehicles.Queries.Stats;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Queries;

public class GetVehicleStatsByMileageQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _repositoryMock = new();
    private readonly GetVehicleStatsByMileageQueryHandler _handler;

    public GetVehicleStatsByMileageQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_repositoryMock.Object);
        _handler = new GetVehicleStatsByMileageQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnStatsDtoFromRepository()
    {
        var stats = new List<VehicleStatsByMileageDto> { new("0-50k", 0, 50000, 3) };
        _repositoryMock.Setup(r => r.GetStatsByMileageAsync()).ReturnsAsync(stats);

        var result = await _handler.Handle(new GetVehicleStatsByMileageQuery(), CancellationToken.None);

        Assert.Same(stats, result);
    }
}
