using Auctioneer.Application.Common.Extensions;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Auctioneer.Application.Features.Members;

public class MemberRepository : IRepository<Member>
{
    private readonly IMongoCollection<Member> _memberCollection;
    private readonly IUnitOfWork _unitOfWork;

    public MemberRepository(IOptions<AuctioneerDatabaseSettings> dbSettings, IUnitOfWork unitOfWork)
    {
        var mongoClient = new MongoClient(dbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(dbSettings.Value.DatabaseName);
        _memberCollection = mongoDatabase.GetCollection<Member>(dbSettings.Value.MemberCollectionName);
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Member>> GetAsync() =>
        await _memberCollection.Find(_ => true).ToListAsync();

    public async Task<(int totalPages, List<Member> data)> GetAsync(int page, int pageSize) =>
        await _memberCollection.AggregateByPage(
            Builders<Member>.Filter.Empty,
            Builders<Member>.Sort.Ascending(m => m.FirstName),
            page,
            pageSize);

    public async Task<Member?> GetAsync(Guid id) =>
        await _memberCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Member newEntity, CancellationToken cancellationToken)
    {
        Action operation = async () =>
            await _memberCollection.InsertOneAsync(_unitOfWork.Session as IClientSessionHandle, newEntity,
                cancellationToken: cancellationToken);
        _unitOfWork.AddOperation(operation);
    }

    public async Task UpdateAsync(Guid id, Member updatedEntity, CancellationToken cancellationToken)
    {
        Action operation = async () =>
            await _memberCollection.ReplaceOneAsync(_unitOfWork.Session as IClientSessionHandle,
                x => x.Id == id,
                updatedEntity, cancellationToken: cancellationToken);
        _unitOfWork.AddOperation(operation);
    }


    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Action operation = async () =>
            await _memberCollection.DeleteOneAsync(_unitOfWork.Session as IClientSessionHandle, x => x.Id == id,
                cancellationToken: cancellationToken);
        _unitOfWork.AddOperation(operation);
    }
}