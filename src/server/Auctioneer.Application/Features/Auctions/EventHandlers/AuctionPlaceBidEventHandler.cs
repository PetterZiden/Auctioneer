using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Auctions.Commands;
using Auctioneer.Application.Infrastructure.Messaging.MassTransit;
using Auctioneer.Application.Infrastructure.Messaging.RabbitMq;
using Auctioneer.MessagingContracts.Email;
using Auctioneer.MessagingContracts.Notification;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.EventHandlers;

public class AuctionPlaceBidEventHandler(
    ILogger<AuctionPlaceBidEventHandler> logger,
    IServiceScopeFactory serviceScopeFactory)
    : INotificationHandler<DomainEventNotification<AuctionPlaceBidEvent>>
{
    public Task Handle(DomainEventNotification<AuctionPlaceBidEvent> notification, CancellationToken cancellationToken)
    {
        try
        {
            var domainEvent = notification.DomainEvent;

            using var scope = serviceScopeFactory.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var messageProducer = scopedServices.GetRequiredService<IMessageProducer>();
            var notificationProducer = scopedServices.GetRequiredService<INotificationProducer>();

            messageProducer.PublishMessage(new Message<PlaceBidMessage>
            {
                Queue = RabbitMqSettings.PlaceBidQueue,
                Exchange = RabbitMqSettings.AuctionExchange,
                ExchangeType = RabbitMqSettings.AuctionExchangeType,
                RouteKey = RabbitMqSettings.PlaceBidRouteKey,
                Data = new PlaceBidMessage(
                    domainEvent.PlaceBidDto.AuctionTitle,
                    domainEvent.PlaceBidDto.AuctionOwnerName,
                    domainEvent.PlaceBidDto.AuctionOwnerEmail,
                    domainEvent.PlaceBidDto.Bid,
                    domainEvent.PlaceBidDto.BidderName,
                    domainEvent.PlaceBidDto.BidderEmail,
                    domainEvent.PlaceBidDto.TimeStamp,
                    domainEvent.PlaceBidDto.AuctionUrl
                )
            });

            notificationProducer.PublishNotification(new Notification<PlaceBidNotification>
            {
                Data = new PlaceBidNotification(
                    domainEvent.PlaceBidDto.AuctionOwnerId,
                    domainEvent.PlaceBidDto.AuctionTitle,
                    domainEvent.PlaceBidDto.BidderName,
                    domainEvent.PlaceBidDto.Bid)
            });

            logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}",
                domainEvent.GetType().Name,
                domainEvent.DomainEventId);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when publishing Rate Member Domain Event with exception: {ExMessage}",
                ex.Message);
            return Task.FromCanceled(cancellationToken);
        }
    }
}