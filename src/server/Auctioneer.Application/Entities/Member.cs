using System.Collections.ObjectModel;
using Ardalis.GuardClauses;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Errors;
using Auctioneer.Application.Common.Guards;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.ValueObjects;
using FluentResults;
using MongoDB.Bson.Serialization.Attributes;

namespace Auctioneer.Application.Entities;

public class Member : AuditableEntity, IAggregateRoot
{
    [BsonId] public Guid Id { get; private init; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Address Address { get; private set; }
    public Email Email { get; private set; }
    public string PhoneNumber { get; private set; }

    private List<Rating> _ratings = [];
    private List<Bid> _bids = [];

    public int CurrentRating => _ratings.Count != 0 ? _ratings.Select(r => r.Stars).Sum() / _ratings.Count : 0;
    public int NumberOfRatings => _ratings.Count != 0 ? _ratings.Count : 0;

    public ReadOnlyCollection<Bid> Bids
    {
        get => _bids.AsReadOnly();
        set => _bids = value.ToList();
    }

    public ReadOnlyCollection<Rating> Ratings
    {
        get => _ratings.AsReadOnly();
        set => _ratings = value.ToList();
    }

    public string FullName =>
        $"{(!string.IsNullOrWhiteSpace(FirstName) ? FirstName : "")} {(!string.IsNullOrWhiteSpace(LastName) ? LastName : "")}";

    public static Result<Member> Create(string firstname, string lastname, string email, string phoneNumber,
        string street, string zipcode, string city)
    {
        try
        {
            var member = new Member
            {
                Id = Guid.NewGuid(),
                FirstName = Guard.Against.NullOrWhiteSpace(firstname, nameof(FirstName), "First name can not be empty"),
                LastName = Guard.Against.NullOrWhiteSpace(lastname, nameof(LastName), "Last name can not be empty"),
                Address = new Address(street, zipcode, city),
                Email = new Email(email),
                PhoneNumber =
                    Guard.Against.NullOrWhiteSpace(phoneNumber, nameof(PhoneNumber), "Phone number can not be empty"),
                Created = DateTimeOffset.Now,
                LastModified = null
            };
            return Result.Ok(member);
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }

    public Result ChangeEmail(Email email)
    {
        try
        {
            Email = Guard.Against.IsNullOrWhitespaceOrSameAsCurrent(email, Email, nameof(Email),
                "Email can not be the same as current email");
            LastModified = DateTimeOffset.Now;
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }

    public Result ChangePhoneNumber(string phoneNumber)
    {
        try
        {
            PhoneNumber = Guard.Against.IsNullOrWhitespaceOrSameAsCurrent(phoneNumber, PhoneNumber, nameof(PhoneNumber),
                "Phone number can not be the same as current phone number");
            LastModified = DateTimeOffset.Now;
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }

    public Result ChangeFirstName(string firstName)
    {
        try
        {
            FirstName = Guard.Against.IsNullOrWhitespaceOrSameAsCurrent(firstName, FirstName, nameof(FirstName),
                "First name can not be the same as current first name");
            LastModified = DateTimeOffset.Now;
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }

    public Result ChangeLastName(string lastName)
    {
        try
        {
            LastName = Guard.Against.IsNullOrWhitespaceOrSameAsCurrent(lastName, LastName, nameof(LastName),
                "Last name can not be the same as current last name");
            LastModified = DateTimeOffset.Now;
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }

    public Result ChangeAddress(Address address)
    {
        try
        {
            Address = Guard.Against.IsSameAsCurrent(address, Address, nameof(Address),
                "Address can not be the same as current address");
            LastModified = DateTimeOffset.Now;
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }

    public Result AddBid(Guid auctionId, decimal bidPrice, DateTimeOffset timeStamp)
    {
        try
        {
            var bid = new Bid
            {
                AuctionId = Guard.Against.NullOrEmpty(auctionId, nameof(auctionId), "AuctionId can not be empty"),
                MemberId = Id,
                BidPrice = Guard.Against.NegativeOrZero(bidPrice, nameof(bidPrice), "Bid must be greater than 0"),
                TimeStamp = timeStamp
            };

            _bids.Add(bid);
            LastModified = DateTimeOffset.Now;

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }

    public Result Rate(Guid memberId, int stars)
    {
        try
        {
            var rating = new Rating
            {
                RatingFromMemberId = memberId,
                Stars = Guard.Against.OutOfRange(stars, nameof(stars), 1, 5, "Rating must be between 1 and 5")
            };

            _ratings.Add(rating);
            LastModified = DateTimeOffset.Now;

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new BadRequestError(ex.Message));
        }
    }
}