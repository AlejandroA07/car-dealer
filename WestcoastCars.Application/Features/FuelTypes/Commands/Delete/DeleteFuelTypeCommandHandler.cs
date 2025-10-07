using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.FuelTypes.Commands.Delete
{
    public class DeleteFuelTypeCommandHandler : IRequestHandler<DeleteFuelTypeCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteFuelTypeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteFuelTypeCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<FuelType>();
            if (repository is null) throw new InvalidOperationException("Repository for FuelType is not available.");

            var fuelTypeToDelete = await repository.GetByIdAsync(request.Id);

            if (fuelTypeToDelete is null)
            {
                throw new NotFoundException($"FuelType with id '{request.Id}' not found.");
            }

            repository.Remove(fuelTypeToDelete!);

            await _unitOfWork.CompleteAsync();

            return Unit.Value;
        }
    }
}
