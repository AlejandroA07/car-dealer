using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using WestcoastCars.Domain.Events.Manufacturers;

namespace WestcoastCars.Application.Features.Manufacturers.EventHandlers
{
    public class ManufacturerCreatedEventHandler : INotificationHandler<ManufacturerCreatedEvent>
    {
        private readonly ILogger<ManufacturerCreatedEventHandler> _logger;

        public ManufacturerCreatedEventHandler(ILogger<ManufacturerCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ManufacturerCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Domain Event: ManufacturerCreatedEvent. Manufacturer \"{Name}\" ({Id}) was created.", notification.Manufacturer.Name, notification.Manufacturer.Id);

            return Task.CompletedTask;
        }
    }
}
