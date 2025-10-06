
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
            var manufacturerToUpdate = await _unitOfWork.Repository<Manufacturer>().GetByIdAsync(request.Id);

            if (manufacturerToUpdate is null)
            {
                throw new NotFoundException($"Manufacturer with id '{request.Id}' not found.");
            }

            var existing = await _unitOfWork.Repository<Manufacturer>().FirstOrDefaultAsync(m => m.Name.Equals(request.Name, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null && existing.Id != request.Id)
            {
                throw new ConflictException($"Manufacturer with name '{request.Name}' already exists.");
            }

            manufacturerToUpdate.Name = request.Name;

            await _unitOfWork.CompleteAsync();

            return Unit.Value;
        }
    }
}
