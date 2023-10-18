using System.Collections.ObjectModel;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Errors;
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

    private List<Bid> _bids = new();

    public ReadOnlyCollection<Bid> Bids
    {
        get => _bids.AsReadOnly();
        set => _bids = value.ToList();
    }

    public static Auction Create(Guid memberId, string title, string description, DateTimeOffset startTime,
        DateTimeOffset endTime, decimal startingPrice, string imgRoute)
    {
        if (string.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));

        if (string.IsNullOrEmpty(description))
            throw new ArgumentNullException(nameof(description));

        if (startTime < DateTimeOffset.Now)
            throw new ArgumentException("Start time can not be earlier than current day and time");

        if (endTime < DateTimeOffset.Now.AddDays(1))
            throw new ArgumentException("End time can not be earlier than at least one day in the future");

        if (startingPrice <= 0)
            throw new ArgumentException("Starting price must be greater than 0");

        if (string.IsNullOrEmpty(imgRoute))
            throw new ArgumentNullException(nameof(imgRoute));

        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Title = title,
            Description = description,
            StartTime = startTime,
            EndTime = endTime,
            StartingPrice = startingPrice,
            CurrentPrice = startingPrice,
            ImgRoute = imgRoute,
            Created = DateTimeOffset.Now,
            LastModified = null,
        };

        return auction;
    }

    public Result<Bid> PlaceBid(Guid memberId, decimal bidPrice)
    {
        if (bidPrice <= 0)
            return Result.Fail(new BadRequestError("Bid must be greater than 0"));

        if (bidPrice <= CurrentPrice)
            return Result.Fail(new BadRequestError($"Bid must be greater than current price: {CurrentPrice}"));

        var bid = new Bid
        {
            AuctionId = Id,
            MemberId = memberId,
            BidPrice = bidPrice,
            TimeStamp = DateTimeOffset.Now
        };

        _bids.Add(bid);

        CurrentPrice = bidPrice;

        return Result.Ok(bid);
    }

    public Result ChangeDescription(string description)
    {
        if (string.IsNullOrEmpty(description))
            throw new ArgumentNullException(nameof(description));

        if (description.Equals(Description))
            return Result.Fail(new BadRequestError("Description can not be the same as current description"));

        Description = description;

        return Result.Ok();
    }

    public Result ChangeTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));

        if (title.Equals(Title))
            return Result.Fail(new BadRequestError("Title can not be the same as current title"));

        Title = title;

        return Result.Ok();
    }

    public Result ChangeImageRoute(string imgRoute)
    {
        if (string.IsNullOrEmpty(imgRoute))
            throw new ArgumentNullException(nameof(imgRoute));

        if (imgRoute.Equals(ImgRoute))
            return Result.Fail(new BadRequestError("Image route can not be the same as current image route"));

        ImgRoute = imgRoute;

        return Result.Ok();
    }
}