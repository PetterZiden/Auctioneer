namespace Auctioneer.GraphQL.Members.Payloads;

public record UpdateMemberPayload(Guid MemberId, DateTimeOffset UpdatedAt);