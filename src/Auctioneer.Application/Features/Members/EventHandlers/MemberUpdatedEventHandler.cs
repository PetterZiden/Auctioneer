using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Members.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.EventHandlers;

public class MemberUpdatedEventHandler : INotificationHandler<DomainEventNotification<MemberUpdatedEvent>>
{
    private readonly ILogger<MemberUpdatedEventHandler> _logger;

    public MemberUpdatedEventHandler(ILogger<MemberUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<MemberUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}", domainEvent.GetType().Name,
            domainEvent.DomainEventId);

        return Task.CompletedTask;
    }
}