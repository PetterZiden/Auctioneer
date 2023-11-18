using Auctioneer.EmailService.Interfaces;
using Auctioneer.MessagingContracts.Email;

namespace Auctioneer.EmailService.Services;

public class MessageHandlerService : IMessageHandlerService
{
    private readonly ILogger<MessageHandlerService> _logger;

    public MessageHandlerService(ILogger<MessageHandlerService> logger)
    {
        _logger = logger;
    }

    public void RateMemberMessageHandler(RateMemberMessage message)
    {
        _logger.LogInformation(
            "Processed RateMemberMessage: {MessageRatedByName} gave {MessageRatedName} {MessageStars} stars, sending email to: {MessageRatedEmail}",
            message.RatedByName, message.RatedName, message.Stars, message.RatedEmail);
    }

    public void PlaceBidMessageHandler(PlaceBidMessage message)
    {
        _logger.LogInformation(
            "Processed PlaceBidMessage: {MessageBidderName} bid {MessageBid} on {MessageAuctionTitle}, owner {MessageAuctionOwnerName}, sending email to: {MessageAuctionOwnerEmail} and {MessageBidderEmail}",
            message.BidderName, message.Bid, message.AuctionTitle, message.AuctionOwnerName, message.AuctionOwnerEmail,
            message.BidderEmail);
    }
}