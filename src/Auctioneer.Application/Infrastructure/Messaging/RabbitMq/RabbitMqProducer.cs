using System.Text;
using System.Text.Json;
using Auctioneer.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Auctioneer.Application.Infrastructure.Messaging.RabbitMq;

public class RabbitMqProducer : IMessageProducer
{
    private readonly IConfiguration _configuration;

    public RabbitMqProducer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void PublishMessage<T>(IMessage<T> message)
    {
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