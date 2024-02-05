namespace Auctioneer.MessagingContracts.Email;

public record CreateMemberMessage(
    Guid MemberId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber);