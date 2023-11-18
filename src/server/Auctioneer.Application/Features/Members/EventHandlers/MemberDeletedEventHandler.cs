using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Members.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.EventHandlers;

public class MemberDeletedEventHandler : INotificationHandler<DomainEventNotification<MemberDeletedEvent>>
{
    private readonly ILogger<MemberDeletedEventHandler> _logger;

    public MemberDeletedEventHandler(ILogger<MemberDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<MemberDeletedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}", domainEvent.GetType().Name,
            domainEvent.DomainEventId);

        return Task.CompletedTask;
    }
}