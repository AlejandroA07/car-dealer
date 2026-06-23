using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.FuelTypes.Queries.ListAll;

public class ListAllFuelTypesQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ListAllFuelTypesQuery, IEnumerable<NamedObjectDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IEnumerable<NamedObjectDto>> Handle(ListAllFuelTypesQuery request, CancellationToken cancellationToken)
    {
        var fuelTypes = await _unitOfWork.FuelTypeRepository.GetAllAsync();
        return fuelTypes.Select(f => f.ToDto());
    }
}
