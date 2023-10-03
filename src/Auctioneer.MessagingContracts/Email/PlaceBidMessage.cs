namespace Auctioneer.MessagingContracts.Email;

public record PlaceBidMessage(
    string AuctionTitle,
    string AuctionOwnerName,
    string AuctionOwnerEmail,
    decimal Bid,
    string BidderName,
    string BidderEmail,
    DateTimeOffset TimeStamp,
    string AuctionUrl
);