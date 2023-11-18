using Auctioneer.Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Infrastructure.Messaging.MassTransit;

public class MassTransitProducer : INotificationProducer
{
    private readonly IPublishEndpoint _publisher;
    private readonly ILogger<MassTransitProducer> _logger;

    public MassTransitProducer(IPublishEndpoint publisher, ILogger<MassTransitProducer> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async void PublishNotification<T>(INotification<T> notification)
    {
        _logger.LogInformation("Publishing message notification-service");
        await _publisher.Publish(notification.Data);
    }
}