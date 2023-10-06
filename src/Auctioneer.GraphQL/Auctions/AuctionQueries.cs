using Auctioneer.Application.Common.Extensions;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Dto;
using HotChocolate.Execution;

namespace Auctioneer.GraphQL.Auctions;

[ExtendObjectType("Query")]
public class AuctionQueries
{
    public async Task<List<AuctionDto>> GetAuctions([Service] IRepository<Auction> auctionRepository)
    {
        var result = await auctionRepository.GetAsync();

        if (result is null)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage("No auction found.")
                    .SetCode("NOT_FOUND")
                    .Build());
        }

        return result.ToDtos();
    }

    public async Task<AuctionDto> GetAuction([Service] IRepository<Auction> auctionRepository, Guid auctionId)
    {
        var result = await auctionRepository.GetAsync(auctionId);

        if (result is null)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage("No auction found.")
                    .SetCode("NOT_FOUND")
                    .Build());
        }

        return result.ToDto();
    }
}