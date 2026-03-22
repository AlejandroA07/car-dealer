using MediatR;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Domain.Common.Enums;

namespace WestcoastCars.Application.Features.ServiceBookings.Commands.Create
{
    public class CreateServiceBookingCommandHandler : IRequestHandler<CreateServiceBookingCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateServiceBookingCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CreateServiceBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = new ServiceBooking
            {
                VehicleRegistrationNumber = request.VehicleRegistrationNumber,
                ServiceType = request.ServiceType,
                BookingDate = request.BookingDate,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                Description = request.Description,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ServiceBookingRepository.AddAsync(booking);

            if (await _unitOfWork.CompleteAsync() > 0)
            {
                return booking.Id;
            }

            throw new System.Exception("Misslyckades att skapa servicebokning");
        }
    }
}
