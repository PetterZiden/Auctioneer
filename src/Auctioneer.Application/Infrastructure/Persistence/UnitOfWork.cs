using Auctioneer.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Auctioneer.Application.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ILogger<UnitOfWork> _logger;
    private IClientSessionHandle session { get; }
    public IDisposable Session => session;

    private List<Action> _operations { get; set; }

    public UnitOfWork(IOptions<AuctioneerDatabaseSettings> dbSettings, ILogger<UnitOfWork> logger)
    {
        _logger = logger;
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        session = mongoClient.StartSession();

        _operations = new List<Action>();
    }

    public void AddOperation(Action operation)
    {
        _operations.Add(operation);
    }

    public void CleanOperations()
    {
        _operations.Clear();
    }

    public async Task SaveAsync()
    {
        try
        {
            session.StartTransaction();

            _operations.ForEach(o => { o.Invoke(); });

            await session.CommitTransactionAsync();

            CleanOperations();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}