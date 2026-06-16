using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.FuelTypes.Queries.ListAll;

public class ListAllFuelTypesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<ListAllFuelTypesQuery, IEnumerable<NamedObjectDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<NamedObjectDto>> Handle(ListAllFuelTypesQuery request, CancellationToken cancellationToken)
    {
        var fuelTypes = await _unitOfWork.FuelTypeRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<NamedObjectDto>>(fuelTypes);
    }
}
