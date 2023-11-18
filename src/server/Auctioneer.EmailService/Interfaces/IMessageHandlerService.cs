using Auctioneer.MessagingContracts.Email;

namespace Auctioneer.EmailService.Interfaces;

public interface IMessageHandlerService
{
    void RateMemberMessageHandler(RateMemberMessage message);
    void PlaceBidMessageHandler(PlaceBidMessage message);
}