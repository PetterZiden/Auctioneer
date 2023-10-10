namespace Auctioneer.Application.Features.Members.Dto;

public class RateMemberDto
{
    public string RatedName { get; set; }
    public Guid RatedMemberId { get; set; }
    public string RatedEmail { get; set; }
    public string RatedByName { get; set; }
    public int Stars { get; set; }
}