namespace Auctioneer.Application.Common.Models;

public class Address : ValueObject
{
    public string Street { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string City { get; set; } = null!;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        throw new NotImplementedException();
    }
}