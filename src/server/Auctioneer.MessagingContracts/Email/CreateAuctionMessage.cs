namespace Auctioneer.MessagingContracts.Email;

public record CreateAuctionMessage(
    Guid AuctionId,
    Guid MemberId,
    string MemberName,
    string Email,
    string Title,
    string Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal StartingPrice
);