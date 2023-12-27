using Auctioneer.MessagingContracts.Notification;
using MassTransit;

namespace Auctioneer.NotificationService.Consumers;

public class PlaceBidNotificationConsumer(ILogger<PlaceBidNotificationConsumer> logger)
    : IConsumer<PlaceBidNotification>
{
    public async Task Consume(ConsumeContext<PlaceBidNotification> context)
    {
        try
        {
            var dto = context.Message;
            //TODO send notification
            logger.LogInformation("{DtoBidderName} placed a bid ({DtoBid} kr) on {DtoAuctionTitle}",
                dto.BidderName,
                dto.Bid,
                dto.AuctionTitle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }
}