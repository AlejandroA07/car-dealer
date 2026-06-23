using MediatR;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Mappings;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;

namespace WestcoastCars.Application.Features.Manufacturers.Commands.Create;

public class CreateManufacturerCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateManufacturerCommand, NamedObjectDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<NamedObjectDto> Handle(CreateManufacturerCommand request, CancellationToken cancellationToken)
    {
        var manufacturerToAdd = new Manufacturer { Name = request.Name };
        await _unitOfWork.ManufacturerRepository.AddAsync(manufacturerToAdd);

        await _unitOfWork.CompleteOrThrowAsync("Failed to create manufacturer");
        return manufacturerToAdd.ToDto();
    }
}
