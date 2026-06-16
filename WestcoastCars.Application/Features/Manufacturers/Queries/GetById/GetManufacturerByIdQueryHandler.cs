using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Manufacturers.Queries.GetById;

public class GetManufacturerByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetManufacturerByIdQuery, NamedObjectDto?>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<NamedObjectDto?> Handle(GetManufacturerByIdQuery request, CancellationToken cancellationToken)
    {
        var manufacturer = await _unitOfWork.ManufacturerRepository.GetByIdAsync(request.Id);
        if (manufacturer is null) return null;
        return _mapper.Map<NamedObjectDto>(manufacturer);
    }
}
