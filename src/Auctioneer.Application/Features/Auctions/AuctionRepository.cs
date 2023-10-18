using Auctioneer.Application.Common.Extensions;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Auctioneer.Application.Features.Auctions;

public class AuctionRepository : IRepository<Auction>
{
    private readonly IMongoCollection<Auction> _auctionCollection;
    private readonly IUnitOfWork _unitOfWork;

    public AuctionRepository(IOptions<AuctioneerDatabaseSettings> dbSettings, IUnitOfWork unitOfWork)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _auctionCollection = mongoDatabase.GetCollection<Auction>(dbSettings.Value.AuctionCollectionName);
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Auction>> GetAsync() =>
        await _auctionCollection.Find(_ => true).ToListAsync();

    public async Task<(int totalPages, List<Auction> data)> GetAsync(int page, int pageSize) =>
        await _auctionCollection.AggregateByPage(
            Builders<Auction>.Filter.Empty,
            Builders<Auction>.Sort.Ascending(m => m.StartTime),
            page,
            pageSize);

    public async Task<Auction?> GetAsync(Guid id) =>
        await _auctionCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Auction newEntity, CancellationToken cancellationToken)
    {
        Action operation = async () =>
            await _auctionCollection.InsertOneAsync(_unitOfWork.Session as IClientSessionHandle, newEntity,
                cancellationToken: cancellationToken);
        _unitOfWork.AddOperation(operation);
    }


    public async Task UpdateAsync(Guid id, Auction updatedEntity, CancellationToken cancellationToken)
    {
        Action operation = async () =>
            await _auctionCollection.ReplaceOneAsync(_unitOfWork.Session as IClientSessionHandle, x => x.Id == id,
                updatedEntity, cancellationToken: cancellationToken);
        _unitOfWork.AddOperation(operation);
    }


    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Action operation = async () =>
            await _auctionCollection.DeleteOneAsync(_unitOfWork.Session as IClientSessionHandle, x => x.Id == id,
                cancellationToken: cancellationToken);
        _unitOfWork.AddOperation(operation);
    }
}