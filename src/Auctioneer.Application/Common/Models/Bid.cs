namespace Auctioneer.Application.Common.Models;

public class Bid
{
    public Guid AuctionId { get; set; }
    public Guid MemberId { get; set; }
    public decimal BidPrice { get; set; }
    public DateTimeOffset? TimeStamp { get; set; }
}