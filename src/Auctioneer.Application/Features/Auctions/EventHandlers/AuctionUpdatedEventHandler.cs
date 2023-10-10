using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Auctions.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.EventHandlers;

public class AuctionUpdatedEventHandler : INotificationHandler<DomainEventNotification<AuctionUpdatedEvent>>
{
    private readonly ILogger<AuctionUpdatedEventHandler> _logger;

    public AuctionUpdatedEventHandler(ILogger<AuctionUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<AuctionUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}", domainEvent.GetType().Name,
            domainEvent.DomainEventId);

        return Task.CompletedTask;
    }
}