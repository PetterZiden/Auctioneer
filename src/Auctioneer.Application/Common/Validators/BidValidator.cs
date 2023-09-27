using Auctioneer.Application.Common.Models;
using FluentValidation;

namespace Auctioneer.Application.Common.Validators;

public class BidValidator : AbstractValidator<Bid>
{
    public BidValidator()
    {
        //Todo: fixa all validering
        RuleFor(v => v.AuctionId)
            .NotNull();

        RuleFor(v => v.MemberId)
            .NotNull();

        RuleFor(v => v.BidPrice)
            .GreaterThan(0);
    }
}