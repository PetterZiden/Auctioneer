using System.Reflection;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Features.Members;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Auctioneer.Application.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private IClientSessionHandle session { get; }
    public IDisposable Session => this.session;

    private List<Action> _operations { get; set; }

    public UnitOfWork(IOptions<AuctioneerDatabaseSettings> dbSettings)
    {
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
        session.StartTransaction();

        _operations.ForEach(o => { o.Invoke(); });

        await session.CommitTransactionAsync();

        CleanOperations();
    }
}