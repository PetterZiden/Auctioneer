using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Auctioneer.Application.Common.Interfaces;

public interface IAggregateRoot
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    Guid Id { get; }
}