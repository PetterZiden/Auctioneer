using System.Reflection;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Common.Validators;
using Auctioneer.Application.Features.Auctions.Commands;
using Auctioneer.Application.Features.Auctions.Queries;
using Auctioneer.gRPC.Common;
using Auctioneer.gRPC.Mappers;
using Grpc.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Auctioneer.gRPC.Services;

[Authorize]
public class AuctionService : Auction.AuctionBase
{
    private readonly ILogger<AuctionService> _logger;
    private readonly IMediator _mediator;

    public AuctionService(ILogger<AuctionService> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public override async Task GetAuctions(GetAuctionsRequest request, IServerStreamWriter<AuctionModel> responseStream,
        ServerCallContext context)
    {
        try
        {
            var query = new GetAuctionsQuery();

            var result = await _mediator.Send(query);
            if (!result.IsSuccess)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(result.Errors[0]));
            }

            var auctionModels = Map.AuctionDtoToAuctionModelList(result.Value);

            foreach (var auction in auctionModels)
            {
                await responseStream.WriteAsync(auction);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<AuctionModel> GetAuction(GetAuctionRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var auctionId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "AuctionId was in wrong format"));

            var query = new GetAuctionQuery { Id = auctionId };

            var result = await _mediator.Send(query);
            if (!result.IsSuccess)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(result.Errors[0]));
            }

            return Map.AuctionDtoToAuctionModel(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<CreateAuctionResponse> CreateAuction(AuctionModel request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.MemberId, out var memberId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));

            var command = new CreateAuctionCommand
            {
                MemberId = memberId,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime.ToDateTimeOffset(),
                EndTime = request.EndTime.ToDateTimeOffset(),
                StartingPrice = (decimal)request.StartingPrice,
                ImgRoute = request.ImgRoute
            };

            var validationResult = await new CreateAuctionCommandValidator().ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(validationResult));
            }

            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(result.Errors[0]));
            }

            return new CreateAuctionResponse
            {
                Id = result.Value.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<DeleteAuctionResponse> DeleteAuction(DeleteAuctionRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var auctionId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "AuctionId was in wrong format"));

            var command = new DeleteAuctionCommand
            {
                AuctionId = auctionId
            };

            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(result.Errors[0]));
            }

            return new DeleteAuctionResponse
            {
                Message = $"Auction with id: {auctionId} was successfully deleted"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<UpdateAuctionResponse> UpdateAuction(AuctionModel request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var auctionId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "AuctionId was in wrong format"));

            var command = new UpdateAuctionCommand
            {
                Id = auctionId,
                Title = request.Title,
                Description = request.Description,
                ImgRoute = request.ImgRoute
            };

            var validationResult = await new UpdateAuctionCommandValidator().ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(validationResult));
            }

            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(result.Errors[0]));
            }

            return new UpdateAuctionResponse
            {
                Message = "Auction updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<PlaceBidResponse> PlaceBid(BidModel request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.AuctionId, out var auctionId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "AuctionId was in wrong format"));

            if (!Guid.TryParse(request.MemberId, out var memberId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));

            var bid = new Bid
            {
                AuctionId = auctionId,
                MemberId = memberId,
                BidPrice = (decimal)request.BidPrice
            };

            var validationResult = await new BidValidator().ValidateAsync(bid);
            if (!validationResult.IsValid)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(validationResult));
            }

            var command = new PlaceBidCommand
            {
                AuctionId = auctionId,
                MemberId = memberId,
                BidPrice = bid.BidPrice
            };

            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(result.Errors[0]));
            }

            return new PlaceBidResponse
            {
                Message = $"Member with id: {memberId} placed bid of {request.BidPrice}kr successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}