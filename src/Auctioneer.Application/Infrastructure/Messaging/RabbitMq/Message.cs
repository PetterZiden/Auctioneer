using Auctioneer.Application.Common.Interfaces;

namespace Auctioneer.Application.Infrastructure.Messaging.RabbitMq;

public class Message<T> : IMessage<T>
{
    public string Queue { get; set; }
    public string Exchange { get; set; }
    public string ExchangeType { get; set; }
    public string RouteKey { get; set; }
    public T Data { get; set; }
}