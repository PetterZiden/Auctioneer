using Auctioneer.Application.Features.Auctions.Dto;

namespace Auctioneer.Application.Features.Auctions.Contracts;

public class GetAuctionsByPaginationResponse
{
    public int TotalPages { get; init; }
    public int PageNumber { get; init; }
    public List<AuctionDto> Auctions { get; init; }
}