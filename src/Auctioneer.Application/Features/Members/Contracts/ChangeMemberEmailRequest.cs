namespace Auctioneer.Application.Features.Members.Contracts;

public record ChangeMemberEmailRequest(
    Guid MemberId,
    string Email);