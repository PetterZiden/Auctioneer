using Auctioneer.Application.Common.Models;

namespace Auctioneer.Application.Features.Members.Dto;


public class MemberDto
{
    public Guid? MemberId { get; set; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Street { get; init; }
    public string ZipCode { get; init; }
    public string City { get; init; }
    public string Email { get; init; }
    public string PhoneNumber { get; init; }
    public int CurrentRating { get; set; }
    public int NumberOfRatings { get; set; }
    public List<Bid> Bids { get; set; }
}