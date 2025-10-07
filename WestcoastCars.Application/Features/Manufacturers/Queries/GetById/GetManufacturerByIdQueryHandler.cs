using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Manufacturers.Queries.GetById
{
    public class GetManufacturerByIdQueryHandler : IRequestHandler<GetManufacturerByIdQuery, NamedObjectDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetManufacturerByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<NamedObjectDto?> Handle(GetManufacturerByIdQuery request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<Manufacturer>();
            if (repository is null) throw new InvalidOperationException("Repository for Manufacturer is not available.");

            var manufacturer = await repository.GetByIdAsync(request.Id);
            if (manufacturer is null) return null;

            var result = _mapper.Map<NamedObjectDto>(manufacturer!);
            return result;
        }
    }
}
