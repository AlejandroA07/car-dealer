using AutoMapper;
using Moq;
using WestcoastCars.Application.Features.ServiceBookings.Queries.ListAll;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.ServiceBookings.Queries;

public class ListServiceBookingsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IServiceBookingRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly ListServiceBookingsQueryHandler _handler;

    public ListServiceBookingsQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ServiceBookingRepository).Returns(_repositoryMock.Object);
        _handler = new ListServiceBookingsQueryHandler(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WithMappedItems()
    {
        var pagedBookings = new PagedResult<ServiceBooking>
        {
            Items = [new ServiceBooking()],
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };
        var mappedDtos = new List<ServiceBookingSummaryDto> { new() };

        _repositoryMock
            .Setup(r => r.GetPagedAsync(It.IsAny<PagedQueryDto>(), null))
            .ReturnsAsync(pagedBookings);
        _mapperMock
            .Setup(m => m.Map<List<ServiceBookingSummaryDto>>(pagedBookings.Items))
            .Returns(mappedDtos);

        var result = await _handler.Handle(new ListServiceBookingsQuery(), CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Same(mappedDtos, result.Items);
    }
}
