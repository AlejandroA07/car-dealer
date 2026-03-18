using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Domain.Events.Vehicles;

namespace WestcoastCars.Application.Features.Vehicles.EventHandlers
{
    public class VehicleSoldEventHandler : INotificationHandler<VehicleSoldEvent>
    {
        private readonly ILogger<VehicleSoldEventHandler> _logger;

        public VehicleSoldEventHandler(ILogger<VehicleSoldEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(VehicleSoldEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Domain Event: VehicleSoldEvent. Vehicle \"{RegNo}\" ({Id}) marked as sold.", 
                notification.Vehicle.RegistrationNumber, 
                notification.Vehicle.Id);

            return Task.CompletedTask;
        }
    }
}
