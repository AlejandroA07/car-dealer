
using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.Manufacturers.Commands.Update;

public class UpdateManufacturerCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateManufacturerCommand>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(UpdateManufacturerCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.ManufacturerRepository;

        var manufacturerToUpdate = await repository.GetByIdAsync(request.Id) ?? throw new NotFoundException($"Manufacturer with id '{request.Id}' not found.");
        manufacturerToUpdate.Name = request.Name;

        await _unitOfWork.CompleteOrThrowAsync("Failed to update manufacturer");
    }
}
