using System.Text;
using Auctioneer.EmailService.Interfaces;
using Auctioneer.EmailService.Settings;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Auctioneer.EmailService.Services;

public class RabbitMqService : IRabbitMqService
{
    private readonly ILogger<RabbitMqService> _logger;
    private readonly IConnection _connection;
    private readonly IOptions<CloudAmqpSettings> _settings;
    private readonly IModel _channel;

    public RabbitMqService(IOptions<CloudAmqpSettings> settings, ILogger<RabbitMqService> logger)
    {
        _logger = logger;
        _settings = settings;
        try
        {
            var factory = new ConnectionFactory
            {
                Uri = _settings.Value.Url
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    public void StartListeningOnQueue(string queue, string routeKey, Action<string> messageHandler)
    {
        _channel.ExchangeDeclare(exchange: "auction", type: ExchangeType.Direct);
        _channel.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: queue, exchange: "auction", routeKey);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            messageHandler?.Invoke(message);
        };

        _channel.BasicConsume(queue: queue, autoAck: true, consumer: consumer);
    }

    public void Stop()
    {
        _channel.Close();
        _connection.Close();
    }
}