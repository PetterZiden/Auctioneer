using Auctioneer.MessagingContracts.Email;

namespace Auctioneer.EmailService.Services;

public static class MessageHandlerService
{
    public static void RateMemberMessageHandler(RateMemberMessage message)
    {
        Console.WriteLine(
            $"Processed RateMemberMessage: {message.RatedByName} gave {message.RatedName} {message.Stars} stars, sending email to: {message.RatedEmail}");
    }

    public static void PlaceBidMessageHandler(PlaceBidMessage message)
    {
        Console.WriteLine(
            $"Processed PlaceBidMessage: {message.BidderName} bid {message.Bid} on {message.AuctionTitle}, owner {message.AuctionOwnerName}, sending email to: {message.AuctionOwnerEmail} and {message.BidderEmail}");
    }
}