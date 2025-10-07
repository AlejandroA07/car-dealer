using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Transmissions.Queries.ListAll
{
    public class ListAllTransmissionsQueryHandler : IRequestHandler<ListAllTransmissionsQuery, IEnumerable<NamedObjectDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ListAllTransmissionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NamedObjectDto>> Handle(ListAllTransmissionsQuery request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<TransmissionType>();
            if (repository is null) throw new InvalidOperationException("Repository for TransmissionType is not available.");

            var transmissionTypes = await repository.GetAllAsync();
            return _mapper.Map<IEnumerable<NamedObjectDto>>(transmissionTypes);
        }
    }
}
