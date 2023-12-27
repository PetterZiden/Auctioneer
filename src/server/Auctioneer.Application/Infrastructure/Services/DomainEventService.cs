using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Infrastructure.Services;

public class DomainEventService(ILogger<DomainEventService> logger, IPublisher publisher) : IDomainEventService
{
    public Task Publish(DomainEvent domainEvent)
    {
        logger.LogInformation("Publishing Domain Event: Event - {Name}", domainEvent.GetType().Name);
        return publisher.Publish(GetNotificationFromDomainEvent(domainEvent));
    }

    private static INotification GetNotificationFromDomainEvent(DomainEvent domainEvent) =>
        (INotification)Activator.CreateInstance(
            typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType()), domainEvent);
}