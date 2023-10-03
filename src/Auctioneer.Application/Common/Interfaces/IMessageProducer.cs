namespace Auctioneer.Application.Common.Interfaces;

public interface IMessageProducer
{
    void PublishMessage<T>(IMessage<T> message);
}