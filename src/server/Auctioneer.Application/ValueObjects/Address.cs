using Auctioneer.Application.Common;

namespace Auctioneer.Application.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; }
    public string Zipcode { get; }
    public string City { get; }

    public Address(string street, string zipcode, string city)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentNullException(nameof(street));

        if (string.IsNullOrWhiteSpace(zipcode))
            throw new ArgumentNullException(nameof(zipcode));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentNullException(nameof(city));

        Street = street;
        Zipcode = zipcode;
        City = city;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Zipcode;
    }
}