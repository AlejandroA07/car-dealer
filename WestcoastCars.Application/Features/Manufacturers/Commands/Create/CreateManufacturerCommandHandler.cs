using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Manufacturers.Commands.Create;

public class CreateManufacturerCommandHandler : IRequestHandler<CreateManufacturerCommand, NamedObjectDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateManufacturerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<NamedObjectDto> Handle(CreateManufacturerCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.ManufacturerRepository;

        await repository.ThrowIfNameExistsAsync(request.Name, nameof(Manufacturer));

        var manufacturerToAdd = new Manufacturer { Name = request.Name };
        await repository.AddAsync(manufacturerToAdd);

        await _unitOfWork.CompleteOrThrowAsync("Failed to create manufacturer");
        return _mapper.Map<NamedObjectDto>(manufacturerToAdd);
    }
}
