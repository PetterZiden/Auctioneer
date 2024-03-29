using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Common.Validators;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Dto;
using Auctioneer.Application.Features.Auctions.Errors;
using Auctioneer.Application.Features.Members.Errors;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.Commands;

public class PlaceBidController(ILogger<PlaceBidController> logger) : ApiControllerBase(logger)
{
    [HttpPost("auction/place-bid")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> PlaceBid([FromBody] Bid request, CancellationToken cancellationToken)
    {
        using var _ = AuctioneerMetrics.MeasureRequestDuration();
        try
        {
            var command = new PlaceBidCommand
            {
                AuctionId = request.AuctionId,
                MemberId = request.MemberId,
                BidPrice = request.BidPrice
            };

            var validationResult = await new BidValidator().ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                return BadRequest(errorMessages);
            }

            var result = await Mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Ok();

            return ReturnError(result.Errors.FirstOrDefault() as Error);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
        }
        finally
        {
            AuctioneerMetrics.IncreaseAuctioneerRequestCount();
        }
    }
}

public class PlaceBidCommand : IRequest<Result>
{
    public Guid AuctionId { get; init; }
    public Guid MemberId { get; init; }
    public decimal BidPrice { get; init; }
}

public class PlaceBidCommandHandler(
    IRepository<Auction> auctionRepository,
    IRepository<Member> memberRepository,
    IRepository<DomainEvent> eventRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PlaceBidCommand, Result>
{
    public async Task<Result> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await auctionRepository.GetAsync(request.AuctionId);

            if (auction is null)
                return Result.Fail(new AuctionNotFoundError());

            var bidder = await memberRepository.GetAsync(request.MemberId);

            var auctionOwner = await memberRepository.GetAsync(auction.MemberId);

            if (bidder is null || auctionOwner is null)
                return Result.Fail(new MemberNotFoundError());

            var bidResult = auction.PlaceBid(request.MemberId, request.BidPrice);

            if (!bidResult.IsSuccess)
                return Result.Fail(bidResult.Errors.FirstOrDefault());

            var memberResult = bidder.AddBid(bidResult.Value.AuctionId, bidResult.Value.BidPrice,
                bidResult.Value.TimeStamp.Value);

            if (!memberResult.IsSuccess)
                return memberResult;

            var placeBidDto = new PlaceBidDto
            {
                AuctionOwnerId = auctionOwner.Id,
                AuctionTitle = auction.Title,
                AuctionOwnerName = auctionOwner.FullName,
                AuctionOwnerEmail = auctionOwner.Email.Value,
                Bid = request.BidPrice,
                BidderName = bidder.FullName,
                BidderEmail = bidder.Email.Value,
                TimeStamp = bidResult.Value.TimeStamp.Value,
                AuctionUrl = $"https://localhost:7298/api/auction/{auction.Id}"
            };
            var domainEvent = new AuctionPlaceBidEvent(placeBidDto, EventList.Auction.AuctionPlaceBidEvent);

            await auctionRepository.UpdateAsync(auction.Id, auction, cancellationToken);
            await memberRepository.UpdateAsync(bidder.Id, bidder, cancellationToken);
            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await unitOfWork.SaveAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            unitOfWork.CleanOperations();
            return Result.Fail(new Error(ex.Message));
        }
    }
}

public class AuctionPlaceBidEvent : DomainEvent, INotification
{
    public AuctionPlaceBidEvent(PlaceBidDto placeBidDto, string @event)
    {
        PlaceBidDto = placeBidDto;
        Event = @event;
    }

    public PlaceBidDto PlaceBidDto { get; set; }
}