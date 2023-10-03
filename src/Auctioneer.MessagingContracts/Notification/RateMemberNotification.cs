namespace Auctioneer.MessagingContracts.Notification;

public record RateMemberNotification(
    Guid NotificationForMemberId,
    string RatedByName,
    int Stars
);