namespace Auctioneer.Application.Features.Members.Dto;

public class RateMemberDto
{
    public string RatedName { get; init; }
    public Guid RatedMemberId { get; init; }
    public string RatedEmail { get; init; }
    public string RatedByName { get; init; }
    public int Stars { get; init; }
}