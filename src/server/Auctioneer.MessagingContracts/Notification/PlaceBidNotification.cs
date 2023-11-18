namespace Auctioneer.MessagingContracts.Notification;

public record PlaceBidNotification(
    Guid NotificationForMemberId,
    string AuctionTitle,
    string BidderName,
    decimal Bid
);