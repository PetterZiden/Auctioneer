namespace Auctioneer.Application.Features.Auctions.Dto;

public class PlaceBidDto
{
    public string AuctionTitle { get; init; }
    public Guid AuctionOwnerId { get; init; }
    public string AuctionOwnerName { get; init; }
    public string AuctionOwnerEmail { get; init; }
    public decimal Bid { get; init; }
    public string BidderName { get; init; }
    public string BidderEmail { get; init; }
    public DateTimeOffset TimeStamp { get; init; }
    public string AuctionUrl { get; init; }
}