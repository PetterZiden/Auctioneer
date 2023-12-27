using Auctioneer.Application.Common.Models;

namespace Auctioneer.Application.Features.Auctions.Dto;

public class AuctionDto
{
    public Guid? Id { get; init; }
    public Guid MemberId { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public decimal StartingPrice { get; init; }
    public decimal CurrentPrice { get; init; }
    public string ImgRoute { get; init; }
    public List<Bid> Bids { get; init; }
}