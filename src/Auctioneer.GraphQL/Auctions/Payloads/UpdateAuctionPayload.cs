namespace Auctioneer.GraphQL.Auctions.Payloads;

public record UpdateAuctionPayload(Guid AuctionId, DateTimeOffset UpdatedAt);