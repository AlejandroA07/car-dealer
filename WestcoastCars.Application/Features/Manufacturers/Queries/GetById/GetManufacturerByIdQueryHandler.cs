using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Manufacturers.Queries.GetById;

public class GetManufacturerByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetManufacturerByIdQuery, NamedObjectDto?>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<NamedObjectDto?> Handle(GetManufacturerByIdQuery request, CancellationToken cancellationToken)
    {
        var manufacturer = await _unitOfWork.ManufacturerRepository.GetByIdAsync(request.Id);
        if (manufacturer is null) return null;
        return manufacturer.ToDto();
    }
}
