using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Infrastructure.Services;

public class DomainEventService : IDomainEventService
{
    private readonly ILogger<DomainEventService> _logger;
    private readonly IPublisher _publisher;

    public DomainEventService(ILogger<DomainEventService> logger, IPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    public Task Publish(DomainEvent domainEvent)
    {
        _logger.LogInformation("Publishing Domain Event: Event - {Name}", domainEvent.GetType().Name);
        return _publisher.Publish(GetNotificationFromDomainEvent(domainEvent));
    }

    private static INotification GetNotificationFromDomainEvent(DomainEvent domainEvent) =>
        (INotification)Activator.CreateInstance(
            typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType()), domainEvent);
}