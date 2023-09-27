namespace Auctioneer.Application.Common.Models;

public class Bid
{
    public Guid AuctionId { get; init; }
    public Guid MemberId { get; init; }
    public decimal BidPrice { get; init; }
    public DateTimeOffset TimeStamp { get; set; }
}