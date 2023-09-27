using Auctioneer.Application.Common.Models;

namespace Auctioneer.Application.Features.Auctions.Dto;

public class AuctionDto
{
    public Guid? AuctionId { get; set; }
    public Guid MemberId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public string ImgRoute { get; set; }
    public List<Bid> Bids { get; set; }
}