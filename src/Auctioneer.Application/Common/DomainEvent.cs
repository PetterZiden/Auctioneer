using Auctioneer.Application.Features.Members.Commands;
using MongoDB.Bson.Serialization.Attributes;

namespace Auctioneer.Application.Common;

[BsonDiscriminator(RootClass = true)]
[BsonKnownTypes(typeof(MemberCreatedEvent), typeof(RateMemberEvent))]
public class DomainEvent
{
    protected DomainEvent()
    {
        DateOccurred = DateTimeOffset.Now;
        DomainEventId = Guid.NewGuid();
    }

    [BsonId] public Guid DomainEventId { get; private set; }
    public DateTimeOffset DateOccurred { get; private set; }
    public string Event { get; set; }
}