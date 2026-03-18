using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.ListAll
{
    public class ListAllVehiclesQueryHandler : IRequestHandler<ListAllVehiclesQuery, IEnumerable<VehicleSummaryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ListAllVehiclesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VehicleSummaryDto>> Handle(ListAllVehiclesQuery request, CancellationToken cancellationToken)
        {
            var vehicles = await _unitOfWork.VehicleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<VehicleSummaryDto>>(vehicles);
        }
    }
}
