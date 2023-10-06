namespace Auctioneer.Application.Features.Members.Contracts;

public record CreateMemberRequest(
    string FirstName,
    string LastName,
    string Street,
    string ZipCode,
    string City,
    string Email,
    string PhoneNumber);