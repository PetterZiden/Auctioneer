using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Features.Auctions.Commands;
using Auctioneer.Application.Features.Auctions.Dto;
using Auctioneer.gRPC.Mappers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Auctioneer.gRPC.Services;

public class AuctionService : Auction.AuctionBase
{
    private readonly ILogger<AuctionService> _logger;
    private readonly IRepository<Auctioneer.Application.Entities.Auction> _auctionRepository;
    private readonly IRepository<Auctioneer.Application.Entities.Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuctionService(ILogger<AuctionService> logger, IRepository<Application.Entities.Auction> auctionRepository,
        IRepository<Application.Entities.Member> memberRepository, IRepository<DomainEvent> eventRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _auctionRepository = auctionRepository;
        _memberRepository = memberRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task GetAuctions(GetAuctionsRequest request, IServerStreamWriter<AuctionModel> responseStream,
        ServerCallContext context)
    {
        try
        {
            var auctions = await _auctionRepository.GetAsync();

            if (auctions is null || !auctions.Any())
                throw new RpcException(new Status(StatusCode.NotFound, "Auction not found"));

            var auctionModels = Map.ApplicationAuctionToAuctionModelList(auctions);

            foreach (var auction in auctionModels)
            {
                await responseStream.WriteAsync(auction);
            }
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<AuctionModel> GetAuction(GetAuctionRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var auctionId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "AuctionId was in wrong format"));

            var auction = await _auctionRepository.GetAsync(auctionId);

            if (auction is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Auction not found"));

            return Map.ApplicationAuctionToAuctionModel(auction);
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<CreateAuctionResponse> CreateAuction(AuctionModel request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.MemberId, out var memberId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));

            var auction = Application.Entities.Auction.Create(
                memberId,
                request.Title,
                request.Description,
                request.StartTime.ToDateTimeOffset(),
                request.EndTime.ToDateTimeOffset(),
                (decimal)request.StartingPrice,
                request.ImgRoute);

            var domainEvent = new AuctionCreatedEvent(auction, EventList.Auction.AuctionCreatedEvent);

            await _auctionRepository.CreateAsync(auction);
            await _eventRepository.CreateAsync(domainEvent);
            await _unitOfWork.SaveAsync();

            return new CreateAuctionResponse
            {
                Id = auction.Id.ToString(),
                CreatedAt = Timestamp.FromDateTimeOffset(auction.Created)
            };
        }
        catch (Exception ex)
        {
            _unitOfWork.CleanOperations();
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

            var domainEvent = new AuctionDeletedEvent(auctionId, EventList.Auction.AuctionDeletedEvent);

            await _auctionRepository.DeleteAsync(auctionId);
            await _eventRepository.CreateAsync(domainEvent);
            await _unitOfWork.SaveAsync();

            return new DeleteAuctionResponse
            {
                Message = $"Auction with id: {auctionId} was successfully deleted"
            };
        }
        catch (Exception ex)
        {
            _unitOfWork.CleanOperations();
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<UpdateAuctionResponse> UpdateAuction(AuctionModel request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var auctionId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "AuctionId was in wrong format"));

            var auction = await _auctionRepository.GetAsync(auctionId);

            if (auction is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Auction not found"));

            var domainEvent = new AuctionUpdatedEvent(auction, EventList.Auction.AuctionUpdatedEvent);

            await _auctionRepository.UpdateAsync(auctionId, auction);
            await _eventRepository.CreateAsync(domainEvent);
            await _unitOfWork.SaveAsync();

            return new UpdateAuctionResponse
            {
                Id = request.Id,
                UpdatedAt = auction.LastModified.HasValue
                    ? Timestamp.FromDateTimeOffset(auction.LastModified.Value)
                    : null
            };
        }
        catch (Exception ex)
        {
            _unitOfWork.CleanOperations();
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

            var auction = await _auctionRepository.GetAsync(auctionId);

            if (auction is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Auction not found"));

            var bidder = await _memberRepository.GetAsync(memberId);

            var auctionOwner = await _memberRepository.GetAsync(auction.MemberId);

            if (bidder is null || auctionOwner is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Member not found"));

            var bidResult = auction.PlaceBid(memberId, (decimal)request.BidPrice);

            if (!bidResult.IsSuccess)
                throw new RpcException(new Status(StatusCode.InvalidArgument, bidResult.Errors[0].Message));

            var memberResult = bidder.AddBid(bidResult.Value.AuctionId, bidResult.Value.BidPrice,
                bidResult.Value.TimeStamp.Value);

            if (!memberResult.IsSuccess)
                throw new RpcException(new Status(StatusCode.InvalidArgument, memberResult.Errors[0].Message));

            var placeBidDto = new PlaceBidDto
            {
                AuctionOwnerId = auctionOwner.Id,
                AuctionTitle = auction.Title,
                AuctionOwnerName = auctionOwner.FullName,
                AuctionOwnerEmail = auctionOwner.Email,
                Bid = (decimal)request.BidPrice,
                BidderName = bidder.FullName,
                BidderEmail = bidder.Email,
                TimeStamp = bidResult.Value.TimeStamp.Value,
                AuctionUrl = $"https://localhost:7298/api/auction/{auction.Id}"
            };
            var domainEvent = new AuctionPlaceBidEvent(placeBidDto, EventList.Auction.AuctionPlaceBidEvent);

            await _auctionRepository.UpdateAsync(auction.Id, auction);
            await _memberRepository.UpdateAsync(bidder.Id, bidder);
            await _eventRepository.CreateAsync(domainEvent);
            await _unitOfWork.SaveAsync();

            return new PlaceBidResponse
            {
                Message =
                    $"Member with id: {memberId} placed bid of {request.BidPrice}kr on {auction.Title} successfully"
            };
        }
        catch (Exception ex)
        {
            _unitOfWork.CleanOperations();
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}