using Auctioneer.MessagingContracts.Notification;
using MassTransit;

namespace Auctioneer.NotificationService.Consumers;

public class RateMemberNotificationConsumer(ILogger<RateMemberNotificationConsumer> logger)
    : IConsumer<RateMemberNotification>
{
    public async Task Consume(ConsumeContext<RateMemberNotification> context)
    {
        try
        {
            var dto = context.Message;
            //TODO send notification
            logger.LogInformation("{DtoRatedByName} gave you {DtoStars} stars",
                dto.RatedByName,
                dto.Stars);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }
    }
}