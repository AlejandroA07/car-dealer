using MediatR;
using WestcoastCars.Application.Exceptions;
using WestcoastCars.Application.Interfaces;

namespace WestcoastCars.Application.Features.Vehicles.Commands.MarkAsSold;

public class MarkAsSoldCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<MarkAsSoldCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Unit> Handle(MarkAsSoldCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _unitOfWork.VehicleRepository.GetByIdAsync(request.Id) ?? throw new NotFoundException($"Vehicle with ID {request.Id} not found");
        try
        {
            vehicle.MarkAsSold();
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        _unitOfWork.VehicleRepository.Update(vehicle);

        await _unitOfWork.CompleteOrThrowAsync("Failed to mark vehicle as sold");
        return Unit.Value;
    }
}
