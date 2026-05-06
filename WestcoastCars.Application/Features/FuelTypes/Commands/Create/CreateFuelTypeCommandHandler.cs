using AutoMapper;
using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.FuelTypes.Commands.Create;

public class CreateFuelTypeCommandHandler : IRequestHandler<CreateFuelTypeCommand, NamedObjectDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateFuelTypeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<NamedObjectDto> Handle(CreateFuelTypeCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.FuelTypeRepository;
        if (repository is null) throw new InvalidOperationException("Repository for FuelType is not available.");

        await repository.ThrowIfNameExistsAsync(request.Name, nameof(FuelType));

        var fuelTypeToAdd = new FuelType { Name = request.Name };
        await repository.AddAsync(fuelTypeToAdd);

        await _unitOfWork.CompleteOrThrowAsync("Failed to create fuel type");
        return _mapper.Map<NamedObjectDto>(fuelTypeToAdd);
    }
}
