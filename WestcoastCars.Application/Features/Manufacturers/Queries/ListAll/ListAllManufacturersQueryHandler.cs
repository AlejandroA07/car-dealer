using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Manufacturers.Queries.ListAll
{
    public class ListAllManufacturersQueryHandler : IRequestHandler<ListAllManufacturersQuery, IEnumerable<NamedObjectDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ListAllManufacturersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NamedObjectDto>> Handle(ListAllManufacturersQuery request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<Manufacturer>();
            if (repository is null) throw new InvalidOperationException("Repository for Manufacturer is not available.");

            var manufacturers = await repository.GetAllAsync();
            var result = _mapper.Map<IEnumerable<NamedObjectDto>>(manufacturers);
            return result;
        }
    }
}
