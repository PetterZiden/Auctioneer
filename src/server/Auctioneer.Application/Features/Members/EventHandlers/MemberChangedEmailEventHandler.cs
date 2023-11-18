using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Members.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.EventHandlers;

public class MemberChangedEmailEventHandler : INotificationHandler<DomainEventNotification<MemberChangedEmailEvent>>
{
    private readonly ILogger<MemberChangedEmailEventHandler> _logger;

    public MemberChangedEmailEventHandler(ILogger<MemberChangedEmailEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<MemberChangedEmailEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}", domainEvent.GetType().Name,
            domainEvent.DomainEventId);

        return Task.CompletedTask;
    }
}