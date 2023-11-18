using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Members.Commands;
using Auctioneer.Application.Infrastructure.Messaging.MassTransit;
using Auctioneer.Application.Infrastructure.Messaging.RabbitMq;
using Auctioneer.MessagingContracts.Email;
using Auctioneer.MessagingContracts.Notification;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.EventHandlers;

public class RateMemberEventHandler : INotificationHandler<DomainEventNotification<RateMemberEvent>>
{
    private readonly ILogger<RateMemberEventHandler> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RateMemberEventHandler(ILogger<RateMemberEventHandler> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task Handle(DomainEventNotification<RateMemberEvent> notification, CancellationToken cancellationToken)
    {
        try
        {
            var domainEvent = notification.DomainEvent;

            using var scope = _serviceScopeFactory.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var messageProducer = scopedServices.GetRequiredService<IMessageProducer>();
            var notificationProducer = scopedServices.GetRequiredService<INotificationProducer>();

            messageProducer.PublishMessage(new Message<RateMemberMessage>
            {
                Queue = RabbitMqSettings.RateMemberQueue,
                Exchange = RabbitMqSettings.AuctionExchange,
                ExchangeType = RabbitMqSettings.AuctionExchangeType,
                RouteKey = RabbitMqSettings.RateMemberRouteKey,
                Data = new RateMemberMessage(
                    domainEvent.RateMember.RatedName,
                    domainEvent.RateMember.RatedEmail,
                    domainEvent.RateMember.RatedByName,
                    domainEvent.RateMember.Stars)
            });

            notificationProducer.PublishNotification(new Notification<RateMemberNotification>
            {
                Data = new RateMemberNotification(
                    domainEvent.RateMember.RatedMemberId,
                    domainEvent.RateMember.RatedByName,
                    domainEvent.RateMember.Stars)
            });
            _logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}", domainEvent.GetType().Name,
                domainEvent.DomainEventId);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error when publishing Rate Member Domain Event with exception: {ExMessage}",
                ex.Message);
            return Task.FromCanceled(cancellationToken);
        }
    }
}