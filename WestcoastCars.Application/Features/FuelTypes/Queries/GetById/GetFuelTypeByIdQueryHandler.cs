using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.FuelTypes.Queries.GetById
{
    public class GetFuelTypeByIdQueryHandler : IRequestHandler<GetFuelTypeByIdQuery, NamedObjectDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetFuelTypeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<NamedObjectDto?> Handle(GetFuelTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<FuelType>();
            if (repository is null) throw new InvalidOperationException("Repository for FuelType is not available.");

            var fuelType = await repository.GetByIdAsync(request.Id);
            if (fuelType is null) return null;

            var result = _mapper.Map<NamedObjectDto>(fuelType!);
            return result;
        }
    }
}
