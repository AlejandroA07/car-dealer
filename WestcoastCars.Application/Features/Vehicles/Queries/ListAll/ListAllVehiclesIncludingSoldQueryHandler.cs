using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.ListAll;

public class ListAllVehiclesIncludingSoldQueryHandler : IRequestHandler<ListAllVehiclesIncludingSoldQuery, PagedResult<VehicleSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ListAllVehiclesIncludingSoldQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<VehicleSummaryDto>> Handle(ListAllVehiclesIncludingSoldQuery request, CancellationToken cancellationToken)
    {
        var vehicles = await _unitOfWork.VehicleRepository.GetAllPagedAsync(new PagedQueryDto
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
