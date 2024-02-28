using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Infrastructure.Services;

public class OutboxPublisher(
    ILogger<OutboxPublisher> logger,
    IRepository<DomainEvent> eventRepository,
    IDomainEventService eventService,
    IUnitOfWork unitOfWork)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox publisher background service started...");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var domainEvents = await eventRepository.GetAsync();
                if (domainEvents.Count != 0)
                {
                    foreach (var @event in domainEvents)
                    {
                        await eventService.Publish(@event);
                        await eventRepository.DeleteAsync(@event.DomainEventId, stoppingToken);
                    }

                    await unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in outbox publisher while publishing events");
            }
            finally
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Outbox publisher background service stopped...");

        return base.StopAsync(cancellationToken);
    }
}