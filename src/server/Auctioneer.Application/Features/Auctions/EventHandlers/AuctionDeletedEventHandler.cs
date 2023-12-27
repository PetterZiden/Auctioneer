using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Auctions.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.EventHandlers;

public class AuctionDeletedEventHandler(ILogger<AuctionDeletedEventHandler> logger)
    : INotificationHandler<DomainEventNotification<AuctionDeletedEvent>>
{
    public Task Handle(DomainEventNotification<AuctionDeletedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}",
            domainEvent.GetType().Name,
            domainEvent.DomainEventId);

        return Task.CompletedTask;
    }
}