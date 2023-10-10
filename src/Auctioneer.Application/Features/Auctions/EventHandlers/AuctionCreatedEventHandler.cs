using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Auctions.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.EventHandlers;

public class AuctionCreatedEventHandler : INotificationHandler<DomainEventNotification<AuctionCreatedEvent>>
{
    private readonly ILogger<AuctionCreatedEventHandler> _logger;

    public AuctionCreatedEventHandler(ILogger<AuctionCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<AuctionCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}", domainEvent.GetType().Name,
            domainEvent.DomainEventId);

        return Task.CompletedTask;
    }
}