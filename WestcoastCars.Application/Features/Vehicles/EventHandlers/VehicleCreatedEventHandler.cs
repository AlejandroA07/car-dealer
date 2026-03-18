using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Domain.Events.Vehicles;

namespace WestcoastCars.Application.Features.Vehicles.EventHandlers
{
    public class VehicleCreatedEventHandler : INotificationHandler<VehicleCreatedEvent>
    {
        private readonly ILogger<VehicleCreatedEventHandler> _logger;

        public VehicleCreatedEventHandler(ILogger<VehicleCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(VehicleCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Domain Event: VehicleCreatedEvent. Vehicle \"{RegNo}\" ({Id}) was added to inventory.", 
                notification.Vehicle.RegistrationNumber, 
                notification.Vehicle.Id);

            return Task.CompletedTask;
        }
    }
}
