using MediatR;

namespace WestcoastCars.Application.Features.Vehicles.Commands.PurgeSourceRemoved;

public record PurgeSourceRemovedVehiclesCommand : IRequest<PurgeSourceRemovedVehiclesResult>;
