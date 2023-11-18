namespace Auctioneer.MessagingContracts.Email;

public record RateMemberMessage(
    string RatedName,
    string RatedEmail,
    string RatedByName,
    int Stars
);