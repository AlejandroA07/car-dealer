using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Vehicles.Queries.GetByRegNo
{
    public class GetVehicleByRegNoQueryHandler : IRequestHandler<GetVehicleByRegNoQuery, VehicleDetailsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetVehicleByRegNoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<VehicleDetailsDto> Handle(GetVehicleByRegNoQuery request, CancellationToken cancellationToken)
        {
            var vehicle = await _unitOfWork.VehicleRepository.FindByRegistrationNumberAsync(request.RegistrationNumber);

            if (vehicle == null)
            {
                throw new NotFoundException($"Vehicle with registration number {request.RegistrationNumber} not found");
            }

            return _mapper.Map<VehicleDetailsDto>(vehicle);
        }
    }
}
