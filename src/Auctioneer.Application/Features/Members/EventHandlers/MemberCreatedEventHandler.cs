using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Members.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.EventHandlers;

public class MemberCreatedEventHandler : INotificationHandler<DomainEventNotification<MemberCreatedEvent>>
{
    private readonly ILogger<MemberCreatedEventHandler> _logger;

    public MemberCreatedEventHandler(ILogger<MemberCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<MemberCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}", domainEvent.GetType().Name,
            domainEvent.DomainEventId);

        return Task.CompletedTask;
    }
}