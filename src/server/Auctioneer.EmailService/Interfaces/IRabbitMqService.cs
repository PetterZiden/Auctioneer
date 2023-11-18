namespace Auctioneer.EmailService.Interfaces;

public interface IRabbitMqService
{
    void StartListeningOnQueue(string queueName, string routeKey, Action<string> messageHandler);
    void Stop();
}