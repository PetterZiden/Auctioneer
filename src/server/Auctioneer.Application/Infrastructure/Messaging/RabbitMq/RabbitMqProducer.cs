using System.Text;
using System.Text.Json;
using Auctioneer.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Auctioneer.Application.Infrastructure.Messaging.RabbitMq;

public class RabbitMqProducer : IMessageProducer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqProducer> _logger;

    public RabbitMqProducer(IConfiguration configuration, ILogger<RabbitMqProducer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void PublishMessage<T>(IMessage<T> message)
    {
        _logger.LogInformation("Publishing message to email-service");

        using var channel = RabbitMqChannelFactory.Instance.GetChannel();
        channel.ExchangeDeclare(exchange: message.Exchange, type: message.ExchangeType);
        channel.QueueDeclare(message.Queue, durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBind(queue: message.Queue, exchange: message.Exchange, routingKey: message.RouteKey);

        var json = JsonSerializer.Serialize(message.Data);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(exchange: message.Exchange, routingKey: message.RouteKey, body: body);
    }
}