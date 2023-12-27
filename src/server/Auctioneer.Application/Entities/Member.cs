using System.Collections.ObjectModel;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Errors;
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
        string street,
        string zipcode, string city)
    {
        if (string.IsNullOrWhiteSpace(firstname))
            return Result.Fail(new BadRequestError("First name can not be empty"));

        if (string.IsNullOrWhiteSpace(lastname))
            return Result.Fail(new BadRequestError("Last name can not be empty"));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result.Fail(new BadRequestError("Phone number can not be empty"));

        try
        {
            var member = new Member
            {
                Id = Guid.NewGuid(),
                FirstName = firstname,
                LastName = lastname,
                Address = new Address(street, zipcode, city),
                Email = new Email(email),
                PhoneNumber = phoneNumber,
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
        if (string.IsNullOrWhiteSpace(email.Value))
            return Result.Fail(new BadRequestError("Email can not be empty"));

        if (Email.Equals(email))
            return Result.Fail(new BadRequestError("Email can not be the same as current email"));

        Email = email;
        LastModified = DateTimeOffset.Now;
        return Result.Ok();
    }

    public Result ChangePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result.Fail(new BadRequestError("Phone number can not be empty"));

        if (PhoneNumber.Equals(phoneNumber))
            return Result.Fail(new BadRequestError("Phone number can not be the same as current phone number"));

        PhoneNumber = phoneNumber;
        LastModified = DateTimeOffset.Now;
        return Result.Ok();
    }

    public Result ChangeFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Fail(new BadRequestError("First name can not be empty"));

        if (FirstName.Equals(firstName))
            return Result.Fail(new BadRequestError("First name can not be the same as current first name"));

        FirstName = firstName;
        LastModified = DateTimeOffset.Now;
        return Result.Ok();
    }

    public Result ChangeLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Fail(new BadRequestError("Last name can not be empty"));

        if (LastName.Equals(lastName))
            return Result.Fail(new BadRequestError("Last name can not be the same as current last name"));

        LastName = lastName;
        LastModified = DateTimeOffset.Now;
        return Result.Ok();
    }

    public Result ChangeAddress(Address address)
    {
        if (address == Address)
            return Result.Fail(new BadRequestError("Address can not be the same as current address"));

        Address = address;
        LastModified = DateTimeOffset.Now;
        return Result.Ok();
    }

    public Result AddBid(Guid auctionId, decimal bidPrice, DateTimeOffset timeStamp)
    {
        if (bidPrice <= 0)
            return Result.Fail(new BadRequestError("Bid must be greater than 0"));

        var bid = new Bid
        {
            AuctionId = auctionId,
            MemberId = Id,
            BidPrice = bidPrice,
            TimeStamp = timeStamp
        };

        _bids.Add(bid);
        LastModified = DateTimeOffset.Now;

        return Result.Ok();
    }

    public Result Rate(Guid memberId, int stars)
    {
        if (stars is <= 0 or >= 6)
            return Result.Fail(new BadRequestError("Rating must be between 1 and 5"));

        var rating = new Rating
        {
            RatingFromMemberId = memberId,
            Stars = stars
        };

        _ratings.Add(rating);
        LastModified = DateTimeOffset.Now;

        return Result.Ok();
    }
}