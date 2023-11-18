namespace Auctioneer.GraphQL.Auctions.Payloads;

public record DeleteAuctionPayload(Guid AuctionId, string Message);