using FluentResults;

namespace Auctioneer.Application.Features.Auctions.Errors;

public class AuctionNotFoundError : Error
{
    public AuctionNotFoundError() : base("No auction found")
    {
        Metadata.Add("HttpStatusCode", "404");
    }
}