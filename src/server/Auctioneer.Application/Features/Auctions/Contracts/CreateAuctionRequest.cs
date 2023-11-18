namespace Auctioneer.Application.Features.Auctions.Contracts;

public record CreateAuctionRequest(
    Guid MemberId,
    string Title,
    string Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal StartingPrice,
    string ImgRoute);