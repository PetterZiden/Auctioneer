using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.GraphQL.Auctions.Inputs;
using Auctioneer.GraphQL.Auctions.Payloads;
using HotChocolate.Execution;

namespace Auctioneer.GraphQL.Auctions;

[ExtendObjectType("Mutation")]
public class AuctionMutations
{
    public async Task<CreateAuctionPayload> CreateAuction(CreateAuctionInput input,
        [Service] IRepository<Auction> auctionRepository)
    {
        try
        {
            var auction = Auction.Create(
                input.MemberId,
                input.Title,
                input.Description,
                input.StartTime,
                input.EndTime,
                input.StartingPrice,
                input.ImgRoute
            );

            await auctionRepository.CreateAsync(auction);

            return new CreateAuctionPayload(auction.Id, auction.Created);
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<UpdateAuctionPayload> UpdateAuction(UpdateAuctionInput input,
        [Service] IRepository<Auction> auctionRepository)
    {
        try
        {
            var auction = await auctionRepository.GetAsync(input.AuctionId);

            if (auction is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No auction found.")
                        .SetCode("NOT_FOUND")
                        .Build());
            }

            await auctionRepository.UpdateAsync(input.AuctionId, auction);

            return new UpdateAuctionPayload(auction.Id, auction.LastModified.Value);
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<DeleteAuctionPayload> DeleteAuction(DeleteAuctionInput input,
        [Service] IRepository<Auction> auctionRepository)
    {
        try
        {
            await auctionRepository.DeleteAsync(input.AuctionId);

            return new DeleteAuctionPayload(input.AuctionId, "Auction deleted successfully");
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<PlaceBidPayload> PlaceBid(PlaceBidInput input, [Service] IRepository<Auction> auctionRepository,
        [Service] IRepository<Member> memberRepository)
    {
        try
        {
            var auction = await auctionRepository.GetAsync(input.AuctionId);

            if (auction is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No auction found.")
                        .SetCode("NOT_FOUND")
                        .Build());
            }

            var bidResult = auction.PlaceBid(input.MemberId, input.BidPrice);

            if (!bidResult.IsSuccess)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(bidResult.Errors?.FirstOrDefault()?.Message)
                        .SetCode("BAD_REQUEST")
                        .Build());
            }


            await auctionRepository.UpdateAsync(auction.Id, auction);

            var bidder = await memberRepository.GetAsync(input.MemberId);

            var auctionOwner = await memberRepository.GetAsync(auction.MemberId);

            if (bidder is null || auctionOwner is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No member found.")
                        .SetCode("NOT_FOND")
                        .Build());
            }

            var memberResult = bidder.AddBid(bidResult.Value.AuctionId, bidResult.Value.BidPrice,
                bidResult.Value.TimeStamp.Value);

            if (!memberResult.IsSuccess)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(memberResult.Errors?.FirstOrDefault()?.Message)
                        .SetCode("BAD_REQUEST")
                        .Build());
            }

            await memberRepository.UpdateAsync(bidder.Id, bidder);

            return new PlaceBidPayload($"Placed bid successfully on auction with id: {auction.Id}");
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }
}