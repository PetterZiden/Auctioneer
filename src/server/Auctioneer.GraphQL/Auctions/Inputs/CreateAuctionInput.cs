namespace Auctioneer.GraphQL.Auctions.Inputs;

public record CreateAuctionInput(
    Guid MemberId,
    string Title,
    string Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal StartingPrice,
    string ImgRoute);