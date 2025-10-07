
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.Manufacturers.Commands.Update
{
    public class UpdateManufacturerCommandHandler : IRequestHandler<UpdateManufacturerCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateManufacturerCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateManufacturerCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<Manufacturer>();
            if (repository is null) throw new InvalidOperationException("Repository for Manufacturer is not available.");

            var manufacturerToUpdate = await repository.GetByIdAsync(request.Id);

            if (manufacturerToUpdate is null)
            {
                throw new NotFoundException($"Manufacturer with id '{request.Id}' not found.");
            }

            var existingRepository = _unitOfWork.Repository<Manufacturer>();
            if (existingRepository is null) throw new InvalidOperationException("Repository for Manufacturer is not available.");

            var existing = await existingRepository.FirstOrDefaultAsync(m => m.Name.Equals(request.Name, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null && existing.Id != request.Id)
            {
                throw new ConflictException($"Manufacturer with name '{request.Name}' already exists.");
            }

            manufacturerToUpdate!.Name = request.Name;

            await _unitOfWork.CompleteAsync();

            return Unit.Value;
        }
    }
}
