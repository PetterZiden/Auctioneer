namespace Auctioneer.GraphQL.Members.Inputs;

public record RateMemberInput(
    Guid RatingForMemberId,
    Guid RatingFromMemberId,
    int Stars);