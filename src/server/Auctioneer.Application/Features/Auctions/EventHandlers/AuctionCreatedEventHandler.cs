using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Auctions.Commands;
using Auctioneer.Application.Infrastructure.Messaging.RabbitMq;
using Auctioneer.MessagingContracts.Email;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.EventHandlers;

public class AuctionCreatedEventHandler(
    ILogger<AuctionCreatedEventHandler> logger,
    IMessageProducer messageProducer)
    : INotificationHandler<DomainEventNotification<AuctionCreatedEvent>>
{
    public Task Handle(DomainEventNotification<AuctionCreatedEvent> notification, CancellationToken cancellationToken)
    {
        try
        {
            var domainEvent = notification.DomainEvent;

            messageProducer.PublishMessage(new Message<CreateAuctionMessage>
            {
                Queue = RabbitMqSettings.CreateAuctionQueue,
                Exchange = RabbitMqSettings.AuctionExchange,
                ExchangeType = RabbitMqSettings.AuctionExchangeType,
                RouteKey = RabbitMqSettings.CreateAuctionRouteKey,
                Data = new CreateAuctionMessage(
                    domainEvent.Auction.Id,
                    domainEvent.Auction.MemberId,
                    domainEvent.Member.FullName,
                    domainEvent.Member.Email.Value,
                    domainEvent.Auction.Title,
                    domainEvent.Auction.Description,
                    domainEvent.Auction.StartTime,
                    domainEvent.Auction.EndTime,
                    domainEvent.Auction.StartingPrice
                )
            });

            logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}",
                domainEvent.GetType().Name,
                domainEvent.DomainEventId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when publishing Create Auction Domain Event with exception: {ExMessage}",
                ex.Message);
            return Task.FromCanceled(cancellationToken);
        }
    }
}