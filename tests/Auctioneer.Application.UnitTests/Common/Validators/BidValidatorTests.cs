using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Common.Validators;
using FluentValidation.TestHelper;

namespace Auctioneer.Application.UnitTests.Common.Validators;

public class BidValidatorTests
{
    private readonly BidValidator _validator = new();

    [Fact]
    public void Should_have_error_when_AuctionId_is_empty()
    {
        var bid = GetValidBid();
        bid.AuctionId = Guid.Empty;

        var result = _validator.TestValidate(bid);
        result.ShouldHaveValidationErrorFor(m => m.AuctionId);
    }

    [Fact]
    public void Should_have_error_when_MemberId_is_empty()
    {
        var bid = GetValidBid();
        bid.MemberId = Guid.Empty;

        var result = _validator.TestValidate(bid);
        result.ShouldHaveValidationErrorFor(m => m.MemberId);
    }

    [Fact]
    public void Should_have_error_when_BidPrice_is_negative_digit()
    {
        var bid = GetValidBid();
        bid.BidPrice = -1;

        var result = _validator.TestValidate(bid);
        result.ShouldHaveValidationErrorFor(m => m.BidPrice);
    }

    private static Bid GetValidBid()
    {
        return new Bid
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            BidPrice = 100,
            TimeStamp = DateTimeOffset.Now
        };
    }
}