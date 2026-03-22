using MediatR;
using System.Collections.Generic;
using WestcoastCars.Contracts.DTOs;

namespace WestcoastCars.Application.Features.ServiceBookings.Queries.ListAll
{
    public class ListServiceBookingsQuery : IRequest<IEnumerable<ServiceBookingSummaryDto>>
    {
    }
}
