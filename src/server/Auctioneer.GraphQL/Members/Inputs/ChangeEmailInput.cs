namespace Auctioneer.GraphQL.Members.Inputs;

public record ChangeEmailInput(Guid MemberId, string Email);