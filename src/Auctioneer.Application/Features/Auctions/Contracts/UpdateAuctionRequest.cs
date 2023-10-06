namespace Auctioneer.Application.Features.Auctions.Contracts;

public record UpdateAuctionRequest(
    Guid AuctionId,
    string Title,
    string Description,
    string ImgRoute);