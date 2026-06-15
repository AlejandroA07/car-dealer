using AutoMapper;
using Moq;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.Vehicles.Queries.GetById;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.Vehicles.Queries;

public class GetVehicleByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IVehicleRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly GetVehicleByIdQueryHandler _handler;

    public GetVehicleByIdQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.VehicleRepository).Returns(_repositoryMock.Object);
        _handler = new GetVehicleByIdQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenVehicleFound()
    {
        var vehicle = new Vehicle
        {
            RegistrationNumber = "ABC123",
            Model = "XC60", ModelYear = 2022, ImageUrl = "img.png", Description = "test",
            Manufacturer = new Manufacturer { Name = "Volvo" },
            FuelType = new FuelType { Name = "Petrol" },
            TransmissionType = new TransmissionType { Name = "Auto" }
        };
        var dto = new VehicleDetailsDto();

        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vehicle);
        _mapperMock.Setup(m => m.Map<VehicleDetailsDto>(vehicle)).Returns(dto);

        var result = await _handler.Handle(new GetVehicleByIdQuery { Id = 1 }, CancellationToken.None);

        Assert.Same(dto, result);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenVehicleNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Vehicle?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(new GetVehicleByIdQuery { Id = 99 }, CancellationToken.None));
    }
}
