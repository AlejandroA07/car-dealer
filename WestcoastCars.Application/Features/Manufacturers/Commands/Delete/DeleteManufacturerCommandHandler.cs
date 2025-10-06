
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.Manufacturers.Commands.Delete
{
    public class DeleteManufacturerCommandHandler : IRequestHandler<DeleteManufacturerCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteManufacturerCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteManufacturerCommand request, CancellationToken cancellationToken)
        {
            var manufacturerToDelete = await _unitOfWork.Repository<Manufacturer>().GetByIdAsync(request.Id);

            if (manufacturerToDelete is null)
            {
                throw new NotFoundException($"Manufacturer with id '{request.Id}' not found.");
            }

            _unitOfWork.Repository<Manufacturer>().Remove(manufacturerToDelete);

            await _unitOfWork.CompleteAsync();

            return Unit.Value;
        }
    }
}
