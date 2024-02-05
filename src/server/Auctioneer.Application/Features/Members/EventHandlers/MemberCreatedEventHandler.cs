using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Features.Members.Commands;
using Auctioneer.Application.Infrastructure.Messaging.RabbitMq;
using Auctioneer.MessagingContracts.Email;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.EventHandlers;

public class MemberCreatedEventHandler(
    ILogger<MemberCreatedEventHandler> logger,
    IMessageProducer messageProducer)
    : INotificationHandler<DomainEventNotification<MemberCreatedEvent>>
{
    public Task Handle(DomainEventNotification<MemberCreatedEvent> notification, CancellationToken cancellationToken)
    {
        try
        {
            var domainEvent = notification.DomainEvent;

            messageProducer.PublishMessage(new Message<CreateMemberMessage>
            {
                Queue = RabbitMqSettings.CreateMemberQueue,
                Exchange = RabbitMqSettings.AuctionExchange,
                ExchangeType = RabbitMqSettings.AuctionExchangeType,
                RouteKey = RabbitMqSettings.CreateMemberRouteKey,
                Data = new CreateMemberMessage(
                    domainEvent.Member.Id,
                    domainEvent.Member.FirstName,
                    domainEvent.Member.LastName,
                    domainEvent.Member.Email.Value,
                    domainEvent.Member.PhoneNumber
                )
            });

            logger.LogInformation("Finished Publishing Domain Event: {Name} - {Id}",
                domainEvent.GetType().Name,
                domainEvent.DomainEventId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when publishing Create Member Domain Event with exception: {ExMessage}",
                ex.Message);
            return Task.FromCanceled(cancellationToken);
        }
    }
}