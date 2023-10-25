using Auctioneer.Application.Infrastructure.Persistence;
using FluentValidation;

namespace Auctioneer.Application.Common.Validators;

public class PaginationParamsValidator : AbstractValidator<PaginationParams>
{
    public PaginationParamsValidator()
    {
        RuleFor(v => v.PageNumber)
            .NotEmpty()
            .NotNull()
            .GreaterThan(0);

        RuleFor(v => v.PageSize)
            .NotEmpty()
            .NotNull()
            .GreaterThan(0);
    }
}