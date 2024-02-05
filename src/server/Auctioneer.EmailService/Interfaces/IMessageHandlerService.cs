using Auctioneer.MessagingContracts.Email;

namespace Auctioneer.EmailService.Interfaces;

public interface IMessageHandlerService
{
    Task CreateMemberMessageHandler(CreateMemberMessage message);
    Task CreateAuctionMessageHandler(CreateAuctionMessage message);
    Task RateMemberMessageHandler(RateMemberMessage message);
    Task PlaceBidMessageHandler(PlaceBidMessage message);
}