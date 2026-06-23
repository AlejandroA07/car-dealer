using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Manufacturers.Queries.ListAll;

public class ListAllManufacturersQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ListAllManufacturersQuery, IEnumerable<NamedObjectDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<IEnumerable<NamedObjectDto>> Handle(ListAllManufacturersQuery request, CancellationToken cancellationToken)
    {
        var manufacturers = await _unitOfWork.ManufacturerRepository.GetAllAsync();
        return manufacturers.Select(m => m.ToDto());
    }
}
