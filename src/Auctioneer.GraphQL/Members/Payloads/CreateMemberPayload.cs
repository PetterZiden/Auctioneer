namespace Auctioneer.GraphQL.Members.Payloads;

public record CreateMemberPayload(Guid CreatedMemberId, DateTimeOffset CreatedAt);