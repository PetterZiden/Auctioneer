using Auctioneer.Application.Features.Auctions.Dto;
using Auctioneer.Application.Features.Auctions.Queries;
using Auctioneer.GraphQL.Common;
using HotChocolate.Execution;
using MediatR;

namespace Auctioneer.GraphQL.Auctions;

[ExtendObjectType("Query")]
public class AuctionQueries(ISender mediator, ILogger<AuctionQueries> logger)
{
    private readonly ILogger<AuctionQueries> _logger = logger;

    public async Task<List<AuctionDto>> GetAuctions()
    {
        var query = new GetAuctionsQuery();

        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
        }

        return result.Value;
    }

    public async Task<AuctionDto> GetAuction(Guid auctionId)
    {
        var query = new GetAuctionQuery { Id = auctionId };

        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
        }

        return result.Value;
    }
}