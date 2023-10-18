using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Infrastructure.Services;

public class OutboxPublisher : BackgroundService
{
    private readonly ILogger<OutboxPublisher> _logger;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IDomainEventService _eventService;
    private readonly IUnitOfWork _unitOfWork;

    public OutboxPublisher(ILogger<OutboxPublisher> logger, IRepository<DomainEvent> eventRepository,
        IDomainEventService eventService, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _eventRepository = eventRepository;
        _eventService = eventService;
        _unitOfWork = unitOfWork;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox publisher background service started...");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var domainEvents = await _eventRepository.GetAsync();
                if (domainEvents is not null && domainEvents.Any())
                {
                    foreach (var @event in domainEvents)
                    {
                        await _eventService.Publish(@event);
                        await _eventRepository.DeleteAsync(@event.DomainEventId, stoppingToken);
                    }

                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in publishing events {ExMessage}", ex.Message);
            }
            finally
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Outbox publisher background service stopped...");

        return base.StopAsync(cancellationToken);
    }
}