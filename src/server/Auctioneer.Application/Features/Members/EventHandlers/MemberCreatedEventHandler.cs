using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Members.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.EventHandlers;

public class MemberCreatedEventHandler(ILogger<MemberCreatedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<MemberCreatedEvent>>
{
    public Task Handle(DomainEventNotification<MemberCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}",
            domainEvent.GetType().Name,
            domainEvent.DomainEventId);

        return Task.CompletedTask;
    }
}