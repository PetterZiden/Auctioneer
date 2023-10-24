using System.Collections.ObjectModel;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Errors;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using FluentResults;
using MongoDB.Bson.Serialization.Attributes;

namespace Auctioneer.Application.Entities;

public class Member : AuditableEntity, IAggregateRoot
{
    [BsonId] public Guid Id { get; private init; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Address Address { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }

    private List<Rating> _ratings = new();
    private List<Bid> _bids = new();

    public int CurrentRating => _ratings.Any() ? _ratings.Select(r => r.Stars).Sum() / _ratings.Count : 0;
    public int NumberOfRatings => _ratings.Any() ? _ratings.Count : 0;

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
        $"{(!string.IsNullOrEmpty(FirstName) ? FirstName : "")} {(!string.IsNullOrEmpty(LastName) ? LastName : "")}";

    public static Member Create(string firstname, string lastname, string email, string phoneNumber, string street,
        string zipcode, string city)
    {
        if (string.IsNullOrEmpty(firstname))
            throw new ArgumentNullException(nameof(firstname));

        if (string.IsNullOrEmpty(lastname))
            throw new ArgumentNullException(nameof(lastname));

        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException(nameof(email));

        if (string.IsNullOrEmpty(phoneNumber))
            throw new ArgumentNullException(nameof(phoneNumber));

        if (string.IsNullOrEmpty(street))
            throw new ArgumentNullException(nameof(street));

        if (string.IsNullOrEmpty(zipcode))
            throw new ArgumentNullException(nameof(zipcode));

        if (string.IsNullOrEmpty(city))
            throw new ArgumentNullException(nameof(city));

        var member = new Member
        {
            Id = Guid.NewGuid(),
            FirstName = firstname,
            LastName = lastname,
            Address = new Address
            {
                Street = street,
                ZipCode = zipcode,
                City = city
            },
            Email = email,
            PhoneNumber = phoneNumber,
            Created = DateTimeOffset.Now,
            LastModified = null
        };

        return member;
    }

    public Result ChangeEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException(nameof(email));

        if (Email.Equals(email))
            return Result.Fail(new BadRequestError("Email can not be the same as current email"));

        Email = email;
        LastModified = DateTimeOffset.Now;
        return Result.Ok();
    }

    public Result ChangePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            throw new ArgumentNullException(nameof(phoneNumber));

        if (PhoneNumber.Equals(phoneNumber))
            return Result.Fail(new BadRequestError("Phone number can not be the same as current phone number"));

        PhoneNumber = phoneNumber;
        LastModified = DateTimeOffset.Now;
        return Result.Ok();
    }

    public Result ChangeFirstName(string firstName)
    {
        if (string.IsNullOrEmpty(firstName))
            throw new ArgumentNullException(nameof(firstName));

        if (FirstName.Equals(firstName))
            return Result.Fail(new BadRequestError("First name can not be the same as current first name"));

        FirstName = firstName;
        LastModified = DateTimeOffset.Now;
        return Result.Ok();
    }

    public Result ChangeLastName(string lastName)
    {
        if (string.IsNullOrEmpty(lastName))
            throw new ArgumentNullException(nameof(lastName));

        if (LastName.Equals(lastName))
            return Result.Fail(new BadRequestError("Last name can not be the same as current last name"));

        LastName = lastName;
        LastModified = DateTimeOffset.Now;
        return Result.Ok();
    }

    public Result ChangeAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
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