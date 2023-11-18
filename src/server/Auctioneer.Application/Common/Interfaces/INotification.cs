namespace Auctioneer.Application.Common.Interfaces;

public interface INotification<T>
{
    public T Data { get; set; }
}