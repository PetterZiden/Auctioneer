namespace Auctioneer.Application.Features.Auctions.Dto;

public class PlaceBidDto
{
    public string AuctionTitle { get; set; }
    public Guid AuctionOwnerId { get; set; }
    public string AuctionOwnerName { get; set; }
    public string AuctionOwnerEmail { get; set; }
    public decimal Bid { get; set; }
    public string BidderName { get; set; }
    public string BidderEmail { get; set; }
    public DateTimeOffset TimeStamp { get; set; }
    public string AuctionUrl { get; set; }
}