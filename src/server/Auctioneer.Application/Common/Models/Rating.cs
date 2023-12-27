namespace Auctioneer.Application.Common.Models;

public class Rating
{
    public Guid RatingForMemberId { get; init; }
    public Guid RatingFromMemberId { get; init; }
    public int Stars { get; init; }
}