using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WestcoastCars.Domain.Common;
using WestcoastCars.Domain.Entities;
using WestcoastCars.Infrastructure.Data;

namespace WestcoastCars.Infrastructure.BackgroundJobs;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while processing outbox messages.");
            }
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WestcoastCarsContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var messages = await context.Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(50)
            .ToListAsync(stoppingToken);

        if (!messages.Any()) return;

        foreach (var message in messages)
        {
            try
            {
                var type = GetEventType(message.Type);
                if (type == null)
                {
                    _logger.LogWarning("Unknown event type: {Type}", message.Type);
                    message.Error = $"Unknown event type: {message.Type}";
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, type) as INotification;

                if (domainEvent != null)
                {
                    await mediator.Publish(domainEvent, stoppingToken);
                }

                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox message {Id}", message.Id);
                message.Error = ex.Message;
            }
        }

        await context.SaveChangesAsync(stoppingToken);
    }

    private static Type? GetEventType(string typeName)
    {
        // In a real senior app, we'd use a more robust way to map stable names to types.
        // For now, we'll scan the assembly where DomainEvent is defined.
        return typeof(DomainEvent).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name == typeName);
    }
}
