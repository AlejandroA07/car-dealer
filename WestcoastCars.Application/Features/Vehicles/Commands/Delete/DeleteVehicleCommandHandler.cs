using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.Vehicles.Commands.Delete
{
    public class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteVehicleCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(request.Id);
            if (vehicle == null)
            {
                throw new NotFoundException($"Vehicle with ID {request.Id} not found");
            }

            _unitOfWork.VehicleRepository.Remove(vehicle);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                return Unit.Value;
            }

            throw new Exception("Failed to delete vehicle");
        }
    }
}
