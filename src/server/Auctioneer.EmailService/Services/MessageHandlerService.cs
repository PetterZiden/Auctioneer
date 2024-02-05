using Auctioneer.EmailService.Interfaces;
using Auctioneer.MessagingContracts.Email;

namespace Auctioneer.EmailService.Services;

public class MessageHandlerService(ILogger<MessageHandlerService> logger, IEmailService emailService)
    : IMessageHandlerService
{
    public async Task CreateMemberMessageHandler(CreateMemberMessage message)
    {
        try
        {
            logger.LogInformation(
                "Processing CreateMemberMessage: {Name} created with id: {MemberId}, sending email to: {Email}",
                message.FirstName + " " + message.LastName, message.MemberId, message.Email);

            await emailService.CreateNewMemberMailMessage(message);
        }
        catch (Exception ex)
        {
            logger.LogError("Could not process CreateMemberMessage");
        }
    }

    public async Task CreateAuctionMessageHandler(CreateAuctionMessage message)
    {
        try
        {
            logger.LogInformation(
                "Processing CreateAuctionMessage: Member with id: {MemberId} and name: {Name} create auction with id: {AuctionId}, sending email to: {Email}",
                message.MemberId, message.MemberName, message.AuctionId, message.Email);

            await emailService.CreateNewAuctionEmail(message);
        }
        catch (Exception ex)
        {
            logger.LogError("Could not process CreateAuctionMessage");
        }
    }

    public async Task RateMemberMessageHandler(RateMemberMessage message)
    {
        try
        {
            logger.LogInformation(
                "Processing RateMemberMessage: {MessageRatedByName} gave {MessageRatedName} {MessageStars} stars, sending email to: {MessageRatedEmail}",
                message.RatedByName, message.RatedName, message.Stars, message.RatedEmail);

            await emailService.RateMemberEmail(message);
        }
        catch (Exception ex)
        {
            logger.LogError("Could not process RateMemberMessage");
        }
    }

    public async Task PlaceBidMessageHandler(PlaceBidMessage message)
    {
        try
        {
            logger.LogInformation(
                "Processing PlaceBidMessage: {MessageBidderName} bid {MessageBid} on {MessageAuctionTitle}, owner {MessageAuctionOwnerName}, sending email to: {MessageAuctionOwnerEmail} and {MessageBidderEmail}",
                message.BidderName, message.Bid, message.AuctionTitle, message.AuctionOwnerName,
                message.AuctionOwnerEmail,
                message.BidderEmail);

            await emailService.PlaceBidEmail(message);
        }
        catch (Exception ex)
        {
            logger.LogError("Could not process PlaceBidMessage");
        }
    }
}