using RabbitMQ.Client;

namespace Auctioneer.Application.Common.Interfaces;

public interface IMessage<T>
{
    public string Queue { get; set; }
    public string Exchange { get; set; }
    public string ExchangeType { get; set; }
    public string RouteKey { get; set; }
    public T Data { get; set; }
}