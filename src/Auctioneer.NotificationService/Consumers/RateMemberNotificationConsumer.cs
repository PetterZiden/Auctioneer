using Auctioneer.MessagingContracts.Notification;
using MassTransit;

namespace Auctioneer.NotificationService.Consumers;

public class RateMemberNotificationConsumer : IConsumer<RateMemberNotification>
{
    private readonly ILogger<RateMemberNotificationConsumer> _logger;

    public RateMemberNotificationConsumer(ILogger<RateMemberNotificationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RateMemberNotification> context)
    {
        try
        {
            var dto = context.Message;
            //TODO send notification
            Console.Write($"{dto.RatedByName} gave you {dto.Stars} stars");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}