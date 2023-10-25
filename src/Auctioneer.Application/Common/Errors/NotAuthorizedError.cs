using FluentResults;

namespace Auctioneer.Application.Common.Errors;

public class NotAuthorizedError : Error
{
    public NotAuthorizedError() : base("Not authorized")
    {
        Metadata.Add("HttpStatusCode", "401");
    }
}