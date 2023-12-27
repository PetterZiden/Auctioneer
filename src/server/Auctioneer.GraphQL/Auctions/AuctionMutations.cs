using System.Reflection;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Common.Validators;
using Auctioneer.Application.Features.Auctions.Commands;
using Auctioneer.GraphQL.Auctions.Inputs;
using Auctioneer.GraphQL.Auctions.Payloads;
using Auctioneer.GraphQL.Common;
using HotChocolate.Execution;
using MediatR;

namespace Auctioneer.GraphQL.Auctions;

[ExtendObjectType("Mutation")]
public class AuctionMutations(ISender mediator, ILogger<AuctionMutations> logger)
{
    public async Task<CreateAuctionPayload> CreateAuction(CreateAuctionInput input, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateAuctionCommand
            {
                MemberId = input.MemberId,
                Title = input.Title,
                Description = input.Description,
                StartTime = input.StartTime,
                EndTime = input.EndTime,
                StartingPrice = input.StartingPrice,
                ImgRoute = input.ImgRoute
            };

            var validationResult = await new CreateAuctionCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(validationResult));
            }

            var result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
            }

            return new CreateAuctionPayload(result.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<UpdateAuctionPayload> UpdateAuction(UpdateAuctionInput input, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateAuctionCommand
            {
                Id = input.AuctionId,
                Title = input.Title,
                Description = input.Description,
                ImgRoute = input.ImgRoute
            };

            var validationResult = await new UpdateAuctionCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(validationResult));
            }

            var result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
            }

            return new UpdateAuctionPayload("Auction updated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<DeleteAuctionPayload> DeleteAuction(DeleteAuctionInput input, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteAuctionCommand
            {
                AuctionId = input.AuctionId
            };

            var result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
            }

            return new DeleteAuctionPayload(input.AuctionId, "Auction deleted successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<PlaceBidPayload> PlaceBid(PlaceBidInput input, CancellationToken cancellationToken)
    {
        try
        {
            var bid = new Bid
            {
                AuctionId = input.AuctionId,
                MemberId = input.MemberId,
                BidPrice = input.BidPrice
            };

            var validationResult = await new BidValidator().ValidateAsync(bid, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(validationResult));
            }

            var command = new PlaceBidCommand
            {
                AuctionId = bid.AuctionId,
                MemberId = bid.MemberId,
                BidPrice = bid.BidPrice
            };

            var result = await mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
            }

            return new PlaceBidPayload($"Placed bid successfully on auction with id: {input.AuctionId}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }
}