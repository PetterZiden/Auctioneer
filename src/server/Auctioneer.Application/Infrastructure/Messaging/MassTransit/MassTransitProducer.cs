using Auctioneer.Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Infrastructure.Messaging.MassTransit;

public class MassTransitProducer(IPublishEndpoint publisher, ILogger<MassTransitProducer> logger)
    : INotificationProducer
{
    public async void PublishNotification<T>(INotification<T> notification)
    {
        logger.LogInformation("Publishing message notification-service");
        await publisher.Publish(notification.Data);
    }
}