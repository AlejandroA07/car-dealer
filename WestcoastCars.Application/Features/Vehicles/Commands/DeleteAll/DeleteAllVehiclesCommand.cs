using MediatR;

namespace WestcoastCars.Application.Features.Vehicles.Commands.DeleteAll;

public record DeleteAllVehiclesCommand : IRequest<DeleteAllVehiclesResult>;
