using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.ListAll;

public class ListAllVehiclesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<ListAllVehiclesQuery, PagedResult<VehicleSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<PagedResult<VehicleSummaryDto>> Handle(ListAllVehiclesQuery request, CancellationToken cancellationToken)
    {
        var vehicles = await _unitOfWork.VehicleRepository.GetUnsoldAsync(new PagedQueryDto
        {
            Page = request.Page,
            PageSize = request.PageSize
        });

        return new PagedResult<VehicleSummaryDto>
        {
            Items = _mapper.Map<List<VehicleSummaryDto>>(vehicles.Items),
            TotalCount = vehicles.TotalCount,
            Page = vehicles.Page,
            PageSize = vehicles.PageSize
        };
    }
}
