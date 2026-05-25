using Moq;
using WestcoastCars.Application.Common.Interfaces.Services;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Features.ServiceBookings.Queries.GetWeekSlots;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Common.Enums;
using Xunit;

namespace WestcoastCars.Application.Tests.Features.ServiceBookings.Queries;

public class GetWeekSlotsQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IServiceBookingRepository> _repositoryMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();

    public GetWeekSlotsQueryHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.ServiceBookingRepository).Returns(_repositoryMock.Object);
        _dateTimeProviderMock.SetupGet(x => x.LocalNow).Returns(new DateTime(2026, 05, 24, 11, 0, 0));
        _repositoryMock
            .Setup(r => r.GetBookedSlotsForRangeAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new HashSet<(DateOnly Date, TimeSlot Slot)>
            {
                (new DateOnly(2026, 05, 25), TimeSlot.Morning)
            });
    }

    [Fact]
    public async Task Handle_ShouldNormalizeWeekStartToMonday()
    {
        var handler = new GetWeekSlotsQueryHandler(_unitOfWorkMock.Object, _dateTimeProviderMock.Object);

        var result = (await handler.Handle(new GetWeekSlotsQuery { WeekStart = new DateOnly(2026, 05, 27) }, CancellationToken.None)).ToList();

        Assert.Equal(15, result.Count);
        Assert.Contains(result, x => x.Date == new DateOnly(2026, 05, 25) && x.TimeSlot == (int)TimeSlot.Morning && x.IsBooked);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenWeekIsOutOfRange()
    {
        var handler = new GetWeekSlotsQueryHandler(_unitOfWorkMock.Object, _dateTimeProviderMock.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new GetWeekSlotsQuery { WeekStart = new DateOnly(2026, 07, 13) }, CancellationToken.None));
    }
}
