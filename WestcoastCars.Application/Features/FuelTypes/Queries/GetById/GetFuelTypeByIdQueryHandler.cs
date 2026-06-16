using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.FuelTypes.Queries.GetById;

public class GetFuelTypeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetFuelTypeByIdQuery, NamedObjectDto?>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<NamedObjectDto?> Handle(GetFuelTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var fuelType = await _unitOfWork.FuelTypeRepository.GetByIdAsync(request.Id);
        if (fuelType is null) return null;
        return _mapper.Map<NamedObjectDto>(fuelType);
    }
}
