namespace Auctioneer.GraphQL.Auctions.Inputs;

public record UpdateAuctionInput(
    Guid AuctionId,
    string Title,
    string Description,
    string ImgRoute);