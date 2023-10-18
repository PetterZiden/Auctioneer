using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Auctioneer.Application.Infrastructure.Persistence;

public class EventRepository : IRepository<DomainEvent>
{
    private readonly IMongoCollection<DomainEvent> _eventCollection;
    private readonly IUnitOfWork _unitOfWork;

    public EventRepository(IOptions<AuctioneerDatabaseSettings> dbSettings, IUnitOfWork unitOfWork)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _eventCollection = mongoDatabase.GetCollection<DomainEvent>(dbSettings.Value.EventCollectionName);
        _unitOfWork = unitOfWork;
    }

    public async Task<List<DomainEvent>> GetAsync() =>
        await _eventCollection.Find(_ => true).ToListAsync();

    public Task<(int totalPages, List<DomainEvent> data)> GetAsync(int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<DomainEvent> GetAsync(Guid id) =>
        await _eventCollection.Find(x => x.DomainEventId == id).FirstOrDefaultAsync();

    public async Task CreateAsync(DomainEvent newEntity, CancellationToken cancellationToken)
    {
        var type = newEntity.GetType();
        Action operation = async () =>
            await _eventCollection.InsertOneAsync(_unitOfWork.Session as IClientSessionHandle, newEntity,
                cancellationToken: cancellationToken);
        _unitOfWork.AddOperation(operation);
    }

    public async Task UpdateAsync(Guid id, DomainEvent updatedEntity, CancellationToken cancellationToken)
    {
        Action operation = async () =>
            await _eventCollection.ReplaceOneAsync(_unitOfWork.Session as IClientSessionHandle,
                x => x.DomainEventId == id,
                updatedEntity, cancellationToken: cancellationToken);
        _unitOfWork.AddOperation(operation);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Action operation = async () =>
            await _eventCollection.DeleteOneAsync(_unitOfWork.Session as IClientSessionHandle,
                x => x.DomainEventId == id, cancellationToken: cancellationToken);
        _unitOfWork.AddOperation(operation);
    }
}