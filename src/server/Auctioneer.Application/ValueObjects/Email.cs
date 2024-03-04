using Ardalis.GuardClauses;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Guards;

namespace Auctioneer.Application.ValueObjects;

public class Email(string email) : ValueObject
{
    public string Value { get; set; } =
        Guard.Against.IsNullOrWhitespaceOrInvalidEmail(email, nameof(email), $"{nameof(email)} is not a valid email");

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}