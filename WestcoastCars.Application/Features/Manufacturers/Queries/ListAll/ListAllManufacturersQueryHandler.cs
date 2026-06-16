using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.Manufacturers.Queries.ListAll;

public class ListAllManufacturersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<ListAllManufacturersQuery, IEnumerable<NamedObjectDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<NamedObjectDto>> Handle(ListAllManufacturersQuery request, CancellationToken cancellationToken)
    {
        var manufacturers = await _unitOfWork.ManufacturerRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<NamedObjectDto>>(manufacturers);
    }
}
