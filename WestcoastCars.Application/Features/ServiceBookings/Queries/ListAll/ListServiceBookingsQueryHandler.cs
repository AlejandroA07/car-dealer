using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.ServiceBookings.Queries.ListAll;

public class ListServiceBookingsQueryHandler : IRequestHandler<ListServiceBookingsQuery, PagedResult<ServiceBookingSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ListServiceBookingsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<ServiceBookingSummaryDto>> Handle(ListServiceBookingsQuery request, CancellationToken cancellationToken)
    {
        var paged = await _unitOfWork.ServiceBookingRepository.GetPagedAsync(new PagedQueryDto
        {
            Page = request.Page,
            PageSize = request.PageSize
        });

        return new PagedResult<ServiceBookingSummaryDto>
        {
            Items = _mapper.Map<List<ServiceBookingSummaryDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }
}
