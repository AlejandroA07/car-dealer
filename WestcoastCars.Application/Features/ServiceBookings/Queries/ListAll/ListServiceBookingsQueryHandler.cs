using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.ServiceBookings.Queries.ListAll
{
    public class ListServiceBookingsQueryHandler : IRequestHandler<ListServiceBookingsQuery, IEnumerable<ServiceBookingSummaryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ListServiceBookingsQueryHandler(IUnitOfWork unitOfWork, IMapper _mapper)
        {
            _unitOfWork = unitOfWork;
            this._mapper = _mapper;
        }

        public async Task<IEnumerable<ServiceBookingSummaryDto>> Handle(ListServiceBookingsQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _unitOfWork.ServiceBookingRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ServiceBookingSummaryDto>>(bookings);
        }
    }
}
