using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.Transmissions.Commands.Update
{
    public class UpdateTransmissionCommandHandler : IRequestHandler<UpdateTransmissionCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTransmissionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateTransmissionCommand request, CancellationToken cancellationToken)
        {
            var transmissionTypeToUpdate = await _unitOfWork.Repository<TransmissionType>()?.GetByIdAsync(request.Id);

            if (transmissionTypeToUpdate is null)
            {
                throw new NotFoundException($"TransmissionType with id '{request.Id}' not found.");
            }

            var existingRepository = _unitOfWork.Repository<TransmissionType>();
            if (existingRepository is null) throw new InvalidOperationException("Repository for TransmissionType is not available.");

            var existing = await existingRepository.FirstOrDefaultAsync(m => m.Name.Equals(request.Name, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null && existing.Id != request.Id)
            {
                throw new ConflictException($"TransmissionType with name '{request.Name}' already exists.");
            }

            transmissionTypeToUpdate!.Name = request.Name;
            _unitOfWork.Repository<TransmissionType>()?.Update(transmissionTypeToUpdate!);

            await _unitOfWork.CompleteAsync();

            return Unit.Value;
        }
    }
}
