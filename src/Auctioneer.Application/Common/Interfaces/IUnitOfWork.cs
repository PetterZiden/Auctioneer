namespace Auctioneer.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IDisposable Session { get; }
    void AddOperation(Action operation);
    void CleanOperations();
    Task SaveAsync();
}