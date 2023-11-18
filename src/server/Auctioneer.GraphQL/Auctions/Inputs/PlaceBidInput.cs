namespace Auctioneer.GraphQL.Auctions.Inputs;

public record PlaceBidInput(
    Guid AuctionId,
    Guid MemberId,
    decimal BidPrice);