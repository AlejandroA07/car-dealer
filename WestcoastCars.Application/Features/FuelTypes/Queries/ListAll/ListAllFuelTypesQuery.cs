using MediatR;
using WestcoastCars.Contracts.DTOs;
using System.Collections.Generic;

namespace WestcoastCars.Application.Features.FuelTypes.Queries.ListAll
{
    public class ListAllFuelTypesQuery : IRequest<IEnumerable<NamedObjectDto>>
    {
    }
}
