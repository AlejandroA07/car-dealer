using Moq;
using WestcoastCars.Application.Features.Vehicles.Queries.Stats;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Queries;

public class GetVehicleStatsSummaryQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _repositoryMock = new();
    private readonly GetVehicleStatsSummaryQueryHandler _handler;

    public GetVehicleStatsSummaryQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_repositoryMock.Object);
        _handler = new GetVehicleStatsSummaryQueryHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSummaryDtoFromRepository()
    {
        var summary = new VehicleStatsSummaryDto(Total: 10, TotalSold: 3, TotalUnsold: 7, TotalSourceRemoved: 0);
        _repositoryMock.Setup(r => r.GetStatsSummaryAsync()).ReturnsAsync(summary);

        var result = await _handler.Handle(new GetVehicleStatsSummaryQuery(), CancellationToken.None);

        Assert.Equal(summary, result);
    }
}
