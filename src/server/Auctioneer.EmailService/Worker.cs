using System.Text.Json;
using Auctioneer.EmailService.Interfaces;
using Auctioneer.MessagingContracts.Email;

namespace Auctioneer.EmailService;

public class Worker(
    ILogger<Worker> logger,
    IRabbitMqService rabbitMqService,
    IMessageHandlerService messageHandlerService)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Background service started...");
        return Task.Run(() =>
        {
            rabbitMqService.StartListeningOnQueue("rate-member-email", "member",
                message =>
                {
                    messageHandlerService.RateMemberMessageHandler(
                        JsonSerializer.Deserialize<RateMemberMessage>(message));
                });

            rabbitMqService.StartListeningOnQueue("place-bid-email", "bid",
                message =>
                {
                    messageHandlerService.PlaceBidMessageHandler(JsonSerializer.Deserialize<PlaceBidMessage>(message));
                });

            stoppingToken.WaitHandle.WaitOne();
        }, stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Background service stopped...");

        rabbitMqService.Stop();
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        Console.WriteLine("Background service stopped...");

        rabbitMqService.Stop();
        base.Dispose();
    }
}