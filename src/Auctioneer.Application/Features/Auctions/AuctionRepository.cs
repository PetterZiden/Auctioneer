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

    public AuctionRepository(IOptions<AuctioneerDatabaseSettings> dbSettings)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _auctionCollection = mongoDatabase.GetCollection<Auction>(dbSettings.Value.AuctionCollectionName);
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

    public async Task CreateAsync(Auction newEntity) =>
        await _auctionCollection.InsertOneAsync(newEntity);

    public async Task UpdateAsync(Guid id, Auction updatedEntity) =>
        await _auctionCollection.ReplaceOneAsync(x => x.Id == id, updatedEntity);

    public async Task DeleteAsync(Guid id) =>
        await _auctionCollection.DeleteOneAsync(x => x.Id == id);
}