using Auctioneer.Application.Features.Auctions.Dto;

namespace Auctioneer.Application.Features.Auctions.Contracts;

public class GetAuctionsByPaginationResponse
{
    public int TotalPages { get; set; }
    public int PageNumber { get; set; }
    public List<AuctionDto> Auctions { get; set; }
}