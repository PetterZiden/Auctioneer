namespace Auctioneer.GraphQL.Members.Payloads;

public record DeleteMemberPayload(Guid MemberId, string Message);