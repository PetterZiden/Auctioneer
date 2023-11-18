using FluentResults;

namespace Auctioneer.Application.Features.Members.Errors;

public class MemberNotFoundError : Error
{
    public MemberNotFoundError() : base("No member found")
    {
        Metadata.Add("HttpStatusCode", "404");
    }
}