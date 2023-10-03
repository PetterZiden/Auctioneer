using Auctioneer.MessagingContracts.Notification;
using MassTransit;

namespace Auctioneer.NotificationService.Consumers;

public class PlaceBidNotificationConsumer : IConsumer<PlaceBidNotification>
{
    private readonly ILogger<PlaceBidNotificationConsumer> _logger;

    public PlaceBidNotificationConsumer(ILogger<PlaceBidNotificationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PlaceBidNotification> context)
    {
        try
        {
            var dto = context.Message;
            //TODO send notification
            Console.Write($"{dto.BidderName} placed a bid ({dto.Bid} kr) on {dto.AuctionTitle}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}