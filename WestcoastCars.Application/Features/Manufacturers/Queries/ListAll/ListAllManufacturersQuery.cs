using MediatR;
using WestcoastCars.Contracts.DTOs;
using System.Collections.Generic;

namespace WestcoastCars.Application.Features.Manufacturers.Queries.ListAll
{
    public class ListAllManufacturersQuery : IRequest<IEnumerable<NamedObjectDto>>
    {
    }
}
