using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Application.Exceptions;

namespace WestcoastCars.Application.Features.Manufacturers.Commands.Create
{
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
            var existing = await _unitOfWork.Repository<Manufacturer>().FirstOrDefaultAsync(m => m.Name.Equals(request.Name, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                throw new ConflictException($"Manufacturer with name '{request.Name}' already exists.");
            }

            var manufacturerToAdd = new Manufacturer { Name = request.Name };
            await _unitOfWork.Repository<Manufacturer>().AddAsync(manufacturerToAdd);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                return _mapper.Map<NamedObjectDto>(manufacturerToAdd);
            }

            return null;
        }
    }
}
