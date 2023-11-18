using Auctioneer.Application.Common.Interfaces;

namespace Auctioneer.Application.Infrastructure.Messaging.MassTransit;

public class Notification<T> : INotification<T>
{
    public T Data { get; set; }
}