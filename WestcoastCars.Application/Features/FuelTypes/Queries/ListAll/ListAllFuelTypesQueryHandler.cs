using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.FuelTypes.Queries.ListAll;

public class ListAllFuelTypesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<ListAllFuelTypesQuery, IEnumerable<NamedObjectDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<NamedObjectDto>> Handle(ListAllFuelTypesQuery request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.FuelTypeRepository ?? throw new InvalidOperationException("Repository for FuelType is not available.");
        var fuelTypes = await repository.GetAllAsync();
        return _mapper.Map<IEnumerable<NamedObjectDto>>(fuelTypes);
    }
}
