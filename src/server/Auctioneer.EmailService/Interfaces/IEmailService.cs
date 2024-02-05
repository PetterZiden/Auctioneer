using Auctioneer.MessagingContracts.Email;

namespace Auctioneer.EmailService.Interfaces;

public interface IEmailService
{
    Task CreateNewMemberMailMessage(CreateMemberMessage message);
    Task CreateNewAuctionEmail(CreateAuctionMessage message);
    Task PlaceBidEmail(PlaceBidMessage message);
    Task RateMemberEmail(RateMemberMessage message);
}