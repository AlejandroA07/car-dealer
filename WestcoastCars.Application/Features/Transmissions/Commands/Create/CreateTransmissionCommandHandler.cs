using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.Transmissions.Commands.Create
{
    public class CreateTransmissionCommandHandler : IRequestHandler<CreateTransmissionCommand, NamedObjectDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateTransmissionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<NamedObjectDto?> Handle(CreateTransmissionCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<TransmissionType>();
            if (repository is null) throw new InvalidOperationException("Repository for TransmissionType is not available.");

            var existing = await repository.FirstOrDefaultAsync(m => m.Name.Equals(request.Name, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                throw new ConflictException($"TransmissionType with name '{request.Name}' already exists.");
            }

            var transmissionTypeToAdd = new TransmissionType { Name = request.Name };
            await repository.AddAsync(transmissionTypeToAdd!);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                return _mapper.Map<NamedObjectDto>(transmissionTypeToAdd!);
            }

            return null;
        }
    }
}
