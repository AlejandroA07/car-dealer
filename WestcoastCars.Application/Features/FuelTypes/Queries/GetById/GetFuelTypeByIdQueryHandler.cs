using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.FuelTypes.Queries.GetById;

public class GetFuelTypeByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetFuelTypeByIdQuery, NamedObjectDto?>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<NamedObjectDto?> Handle(GetFuelTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var fuelType = await _unitOfWork.FuelTypeRepository.GetByIdAsync(request.Id);
        if (fuelType is null) return null;
        return fuelType.ToDto();
    }
}
