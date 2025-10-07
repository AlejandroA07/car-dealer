using MediatR;
using WestcoastCars.Contracts.DTOs;
using System.Collections.Generic;

namespace WestcoastCars.Application.Features.Transmissions.Queries.ListAll
{
    public class ListAllTransmissionsQuery : IRequest<IEnumerable<NamedObjectDto>>
    {
    }
}
