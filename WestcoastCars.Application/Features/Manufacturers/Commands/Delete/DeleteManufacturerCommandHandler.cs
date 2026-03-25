
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

        public async Task Handle(DeleteManufacturerCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.Repository<Manufacturer>();
            if (repository is null) throw new InvalidOperationException("Repository for Manufacturer is not available.");

            var manufacturerToDelete = await repository.GetByIdAsync(request.Id);

            if (manufacturerToDelete is null)
            {
                throw new NotFoundException($"Manufacturer with id '{request.Id}' not found.");
            }

            repository.Remove(manufacturerToDelete!);

            await _unitOfWork.CompleteAsync();
        }
    }
}
