using Auctioneer.Application.Features.Members.Dto;

namespace Auctioneer.Application.Features.Members.Contracts;

public class GetMembersByPaginationResponse
{
    public int TotalPages { get; init; }
    public int PageNumber { get; init; }
    public List<MemberDto> Members { get; init; }
}