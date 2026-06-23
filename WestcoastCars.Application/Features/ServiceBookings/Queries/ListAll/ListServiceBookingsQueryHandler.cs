using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.ServiceBookings.Queries.ListAll;

public class ListServiceBookingsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ListServiceBookingsQuery, PagedResult<ServiceBookingSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<PagedResult<ServiceBookingSummaryDto>> Handle(ListServiceBookingsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.ServiceBookingRepository.GetPagedAsync(new PagedQueryDto
        {
            Page = request.Page,
            PageSize = request.PageSize
        }, request.IsActive);

        return new PagedResult<ServiceBookingSummaryDto>
        {
            Items = paged.Items.Select(b => b.ToDto()).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
}
