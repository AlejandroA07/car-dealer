using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.FuelTypes.Commands.Update
{
    public class UpdateFuelTypeCommandHandler : IRequestHandler<UpdateFuelTypeCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateFuelTypeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateFuelTypeCommand request, CancellationToken cancellationToken)
        {
            var fuelTypeToUpdate = await _unitOfWork.Repository<FuelType>()?.GetByIdAsync(request.Id);

            if (fuelTypeToUpdate is null)
            {
                throw new NotFoundException($"FuelType with id '{request.Id}' not found.");
            }

            var existingRepository = _unitOfWork.Repository<FuelType>();
            if (existingRepository is null) throw new InvalidOperationException("Repository for FuelType is not available.");

            var existing = await existingRepository.FirstOrDefaultAsync(m => m.Name.Equals(request.Name, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null && existing.Id != request.Id)
            {
                throw new ConflictException($"FuelType with name '{request.Name}' already exists.");
            }

            fuelTypeToUpdate!.Name = request.Name;
            _unitOfWork.Repository<FuelType>()?.Update(fuelTypeToUpdate!);

            await _unitOfWork.CompleteAsync();
        }
    }
}
