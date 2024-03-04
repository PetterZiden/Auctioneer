using Ardalis.GuardClauses;
using Auctioneer.Application.Common;

namespace Auctioneer.Application.ValueObjects;

public class Address(string street, string zipcode, string city) : ValueObject
{
    public string Street { get; } = Guard.Against.NullOrWhiteSpace(street, nameof(Street));
    public string Zipcode { get; } = Guard.Against.NullOrWhiteSpace(zipcode, nameof(Zipcode));
    public string City { get; } = Guard.Against.NullOrWhiteSpace(city, nameof(City));

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Zipcode;
    }
}