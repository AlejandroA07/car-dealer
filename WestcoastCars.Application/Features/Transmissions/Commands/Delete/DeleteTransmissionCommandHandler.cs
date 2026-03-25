using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.Transmissions.Commands.Delete
{
    public class DeleteTransmissionCommandHandler : IRequestHandler<DeleteTransmissionCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTransmissionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteTransmissionCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<TransmissionType>();
            if (repository is null) throw new InvalidOperationException("Repository for TransmissionType is not available.");

            var transmissionTypeToDelete = await repository.GetByIdAsync(request.Id);

            if (transmissionTypeToDelete is null)
            {
                throw new NotFoundException($"TransmissionType with id '{request.Id}' not found.");
            }

            repository.Remove(transmissionTypeToDelete!);

            await _unitOfWork.CompleteAsync();
        }
    }
}
