using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Auctions.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.EventHandlers;

public class AuctionDeletedEventHandler : INotificationHandler<DomainEventNotification<AuctionDeletedEvent>>
{
    private readonly ILogger<AuctionDeletedEventHandler> _logger;

    public AuctionDeletedEventHandler(ILogger<AuctionDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<AuctionDeletedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}", domainEvent.GetType().Name,
            domainEvent.DomainEventId);

        return Task.CompletedTask;
    }
}