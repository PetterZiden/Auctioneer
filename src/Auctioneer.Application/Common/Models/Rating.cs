namespace Auctioneer.Application.Common.Models;

public class Rating
{
    public Guid RatingFromMemberId { get; set; }
    public int Stars { get; init; }
}