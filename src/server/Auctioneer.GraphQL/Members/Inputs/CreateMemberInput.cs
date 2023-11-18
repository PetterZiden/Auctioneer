namespace Auctioneer.GraphQL.Members.Inputs;

public record CreateMemberInput(
    string FirstName,
    string LastName,
    string Street,
    string ZipCode,
    string City,
    string Email,
    string PhoneNumber);