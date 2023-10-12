namespace Auctioneer.Application.Common.Helpers;

public static class EventList
{
    public static class Member
    {
        public const string MemberCreatedEvent = "member.created";
        public const string MemberDeletedEvent = "member.deleted";
        public const string MemberUpdatedEvent = "member.updated";
        public const string MemberChangedEmailEvent = "member.updated.email";
        public const string RateMemberEvent = "member.rated";
    }

    public static class Auction
    {
        public const string AuctionCreatedEvent = "auction.created";
        public const string AuctionDeletedEvent = "auction.deleted";
        public const string AuctionUpdatedEvent = "auction.updated";
        public const string AuctionPlaceBidEvent = "auction.updated";
    }
}