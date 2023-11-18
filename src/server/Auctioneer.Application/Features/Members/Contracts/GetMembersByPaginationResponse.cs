using Auctioneer.Application.Features.Members.Dto;

namespace Auctioneer.Application.Features.Members.Contracts;

public class GetMembersByPaginationResponse
{
    public int TotalPages { get; set; }
    public int PageNumber { get; set; }
    public List<MemberDto> Members { get; set; }
}