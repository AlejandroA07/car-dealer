using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.FuelTypes.Commands.Create;

public class CreateFuelTypeCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateFuelTypeCommand, NamedObjectDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<NamedObjectDto> Handle(CreateFuelTypeCommand request, CancellationToken cancellationToken)
    {
        var fuelTypeToAdd = new FuelType { Name = request.Name };
        await _unitOfWork.FuelTypeRepository.AddAsync(fuelTypeToAdd);

        await _unitOfWork.CompleteOrThrowAsync("Failed to create fuel type");
        return fuelTypeToAdd.ToDto();
    }
}
