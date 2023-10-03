using System.Text.Json;
using Auctioneer.EmailService.Interfaces;
using Auctioneer.MessagingContracts.Email;

namespace Auctioneer.EmailService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private readonly IRabbitMqService _rabbitMqService;
    private readonly IMessageHandlerService _messageHandlerService;

    public Worker(ILogger<Worker> logger, IRabbitMqService rabbitMqService,
        IMessageHandlerService messageHandlerService)
    {
        _logger = logger;
        _rabbitMqService = rabbitMqService;
        _messageHandlerService = messageHandlerService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background service started...");
        //Console.WriteLine("Background service started...");
        return Task.Run(() =>
        {
            _rabbitMqService.StartListeningOnQueue("rate-member-email", "member",
                message =>
                {
                    _messageHandlerService.RateMemberMessageHandler(
                        JsonSerializer.Deserialize<RateMemberMessage>(message));
                });

            _rabbitMqService.StartListeningOnQueue("place-bid-email", "bid",
                message =>
                {
                    _messageHandlerService.PlaceBidMessageHandler(JsonSerializer.Deserialize<PlaceBidMessage>(message));
                });

            stoppingToken.WaitHandle.WaitOne();
        }, stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Background service stopped...");
        //Console.WriteLine("Background service stopped...");

        _rabbitMqService.Stop();
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        Console.WriteLine("Background service stopped...");

        _rabbitMqService.Stop();
        base.Dispose();
    }
}