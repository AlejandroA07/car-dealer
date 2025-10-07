using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.FuelTypes.Commands.Create
{
    public class CreateFuelTypeCommandHandler : IRequestHandler<CreateFuelTypeCommand, NamedObjectDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateFuelTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<NamedObjectDto?> Handle(CreateFuelTypeCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<FuelType>();
            if (repository is null) throw new InvalidOperationException("Repository for FuelType is not available.");

            var existing = await repository.FirstOrDefaultAsync(m => m.Name.Equals(request.Name, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                throw new ConflictException($"FuelType with name '{request.Name}' already exists.");
            }

            var fuelTypeToAdd = new FuelType { Name = request.Name };
            await repository.AddAsync(fuelTypeToAdd!);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                return _mapper.Map<NamedObjectDto>(fuelTypeToAdd!);
            }

            return null;
        }
    }
}
