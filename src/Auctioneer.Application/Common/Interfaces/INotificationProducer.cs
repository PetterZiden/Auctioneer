namespace Auctioneer.Application.Common.Interfaces;

public interface INotificationProducer
{
    void PublishNotification<T>(INotification<T> notification);
}