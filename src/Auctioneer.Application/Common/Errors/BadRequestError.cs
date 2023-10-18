using FluentResults;

namespace Auctioneer.Application.Common.Errors;

public class BadRequestError : Error
{
    public BadRequestError(string msg) : base(msg)
    {
        Metadata.Add("HttpStatusCode", "400");
    }
}