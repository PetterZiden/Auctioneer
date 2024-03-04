using System.Collections.ObjectModel;
using Ardalis.GuardClauses;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Errors;
using Auctioneer.Application.Common.Guards;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using FluentResults;

namespace Auctioneer.Application.Entities;

public class Auction : AuditableEntity, IAggregateRoot
{
    public Guid Id { get; private init; }
    public Guid MemberId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public decimal StartingPrice { get; private set; }
    public decimal CurrentPrice { get; private set; }
    public string ImgRoute { get; private set; }

    private List<Bid> _bids = [];

    public ReadOnlyCollection<Bid> Bids
    {
        get => _bids.AsReadOnly();
        set => _bids = value.ToList();
    }

    public static Result<Auction> Create(Guid memberId, string title, string description, DateTimeOffset startTime,
        DateTimeOffset endTime, decimal startingPrice, string imgRoute)
    {
        try
        {
            var auction = new Auction
            {
                Id = Guid.NewGuid(),
                MemberId = Guard.Against.NullOrEmpty(memberId, nameof(MemberId), "MemberId can not be empty"),
                Title = Guard.Against.NullOrWhiteSpace(title, nameof(Title), "Title can not be empty"),
                Description =
                    Guard.Against.NullOrWhiteSpace(description, nameof(Description), "Description can not be empty"),
                StartTime = Guard.Against.DateTimeOffsetLesserThan(startTime, DateTimeOffset.Now, nameof(StartTime),
                    "Start time can not be earlier than current day and time"),
                EndTime = Guard.Against.DateTimeOffsetLesserThan(endTime, DateTimeOffset.Now.AddDays(1),
                    nameof(EndTime), "End time can not be earlier than at least one day in the future"),
                StartingPrice = Guard.Against.NegativeOrZero(startingPrice, nameof(StartingPrice),
                    "Starting price must be greater than 0"),
                CurrentPrice = startingPrice,
                ImgRoute = Guard.Against.NullOrWhiteSpace(imgRoute, nameof(ImgRoute), "Image route can not be empty"),
                Created = DateTimeOffset.Now,
                LastModified = null
            };

            return Result.Ok(auction);
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }

    public Result<Bid> PlaceBid(Guid memberId, decimal bidPrice)
    {
        try
        {
            var bid = new Bid
            {
                AuctionId = Id,
                MemberId = memberId,
                BidPrice = Guard.Against.NegativeOrZeroOrGreaterThan(bidPrice, CurrentPrice, nameof(bidPrice),
                    $"Bid must be greater than current price: {CurrentPrice}"),
                TimeStamp = DateTimeOffset.Now
            };

            _bids.Add(bid);

            CurrentPrice = bidPrice;
            LastModified = DateTimeOffset.Now;

            return Result.Ok(bid);
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }

    public Result ChangeDescription(string description)
    {
        try
        {
            Description = Guard.Against.IsNullOrWhitespaceOrSameAsCurrent(description, nameof(Description),
                "Description can not be the same as current description");
            LastModified = DateTimeOffset.Now;

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }

    public Result ChangeTitle(string title)
    {
        try
        {
            Title = Guard.Against.IsNullOrWhitespaceOrSameAsCurrent(title, nameof(Title),
                "Title can not be the same as current title");
            LastModified = DateTimeOffset.Now;

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }

    public Result ChangeImageRoute(string imgRoute)
    {
        try
        {
            ImgRoute = Guard.Against.IsNullOrWhitespaceOrSameAsCurrent(imgRoute, nameof(ImgRoute),
                "Image route can not be the same as current image route");
            LastModified = DateTimeOffset.Now;

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }
}