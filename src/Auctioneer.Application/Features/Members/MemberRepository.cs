using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Auctioneer.Application.Features.Members;

public class MemberRepository : IRepository<Member>
{
    private readonly IMongoCollection<Member> _memberCollection;

    public MemberRepository(IOptions<AuctioneerDatabaseSettings> dbSettings)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _memberCollection = mongoDatabase.GetCollection<Member>(dbSettings.Value.MemberCollectionName);
    }
    
    public async Task<List<Member>> GetAsync() =>
        await _memberCollection.Find(_ => true).ToListAsync();

    public async Task<Member?> GetAsync(Guid id) =>
        await _memberCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Member newEntity) =>
        await _memberCollection.InsertOneAsync(newEntity);

    public async Task UpdateAsync(Guid id, Member updatedEntity) =>
        await _memberCollection.ReplaceOneAsync(x => x.Id == id, updatedEntity);

    public async Task DeleteAsync(Guid id) =>
        await _memberCollection.DeleteOneAsync(x => x.Id == id);

}