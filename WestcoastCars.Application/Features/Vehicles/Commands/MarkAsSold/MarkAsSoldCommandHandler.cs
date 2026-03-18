using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.Vehicles.Commands.MarkAsSold
{
    public class MarkAsSoldCommandHandler : IRequestHandler<MarkAsSoldCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarkAsSoldCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(MarkAsSoldCommand request, CancellationToken cancellationToken)
        {
            var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(request.Id);
            if (vehicle == null)
            {
                throw new NotFoundException($"Vehicle with ID {request.Id} not found");
            }

            vehicle.IsSold = true;
            _unitOfWork.VehicleRepository.Update(vehicle);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                return Unit.Value;
            }

            throw new Exception("Failed to mark vehicle as sold");
        }
    }
}
