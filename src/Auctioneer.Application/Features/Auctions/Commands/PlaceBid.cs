using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Common.Validators;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Infrastructure.Messaging.MassTransit;
using Auctioneer.Application.Infrastructure.Messaging.RabbitMq;
using Auctioneer.MessagingContracts.Email;
using Auctioneer.MessagingContracts.Notification;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.Commands;

public class PlaceBidController : ApiControllerBase
{
    private readonly ILogger<PlaceBidController> _logger;

    public PlaceBidController(ILogger<PlaceBidController> logger) : base(logger)
    {
        _logger = logger;
    }

    [HttpPost("api/auction/place-bid")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<Guid>> PlaceBid(Bid bid)
    {
        try
        {
            var command = new PlaceBidCommand { Bid = bid };

            var validationResult = await new BidValidator().ValidateAsync(command.Bid);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                return BadRequest(errorMessages);
            }

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                return Ok();

            return ReturnError(result.Errors.FirstOrDefault() as Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
        }
    }
}

public class PlaceBidCommand : IRequest<Result>
{
    public Bid Bid { get; init; }
}

internal sealed class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand, Result>
{
    private readonly IRepository<Auction> _auctionRepository;
    private readonly IRepository<Member> _memberRepository;
    private readonly IMessageProducer _messageProducer;
    private readonly INotificationProducer _notificationProducer;

    public PlaceBidCommandHandler(IRepository<Auction> auctionRepository, IRepository<Member> memberRepository,
        IMessageProducer messageProducer, INotificationProducer notificationProducer)
    {
        _auctionRepository = auctionRepository;
        _memberRepository = memberRepository;
        _messageProducer = messageProducer;
        _notificationProducer = notificationProducer;
    }

    public async Task<Result> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await _auctionRepository.GetAsync(request.Bid.AuctionId);

            if (auction is null)
                return Result.Fail(new Error("No auction found"));

            var bidResult = auction.PlaceBid(request.Bid.MemberId, request.Bid.BidPrice);

            if (!bidResult.IsSuccess)
                return Result.Fail(bidResult.Errors);

            await _auctionRepository.UpdateAsync(auction.Id, auction);

            var bidder = await _memberRepository.GetAsync(request.Bid.MemberId);

            var auctionOwner = await _memberRepository.GetAsync(auction.MemberId);

            if (bidder is null || auctionOwner is null)
                return Result.Fail(new Error("No member found"));

            var memberResult = bidder.AddBid(bidResult.Value.AuctionId, bidResult.Value.BidPrice,
                bidResult.Value.TimeStamp.Value);

            if (!memberResult.IsSuccess)
                return memberResult;

            await _memberRepository.UpdateAsync(bidder.Id, bidder);

            _messageProducer.PublishMessage(new Message<PlaceBidMessage>
            {
                Queue = RabbitMqSettings.PlaceBidQueue,
                Exchange = RabbitMqSettings.AuctionExchange,
                ExchangeType = RabbitMqSettings.AuctionExchangeType,
                RouteKey = RabbitMqSettings.PlaceBidRouteKey,
                Data = new PlaceBidMessage(
                    auction.Title,
                    auctionOwner.FullName,
                    auctionOwner.Email,
                    request.Bid.BidPrice,
                    bidder.FullName,
                    bidder.Email,
                    bidResult.Value.TimeStamp.Value,
                    $"https://localhost:7298/api/auction/{auction.Id}")
            });

            _notificationProducer.PublishNotification(new Notification<PlaceBidNotification>
            {
                Data = new PlaceBidNotification(
                    auctionOwner.Id,
                    auction.Title,
                    bidder.FullName,
                    request.Bid.BidPrice)
            });

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}