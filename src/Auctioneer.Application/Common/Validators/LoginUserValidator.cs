using Auctioneer.Application.Auth.Models;
using FluentValidation;

namespace Auctioneer.Application.Common.Validators;

public class LoginUserValidator : AbstractValidator<LoginUser>
{
    public LoginUserValidator()
    {
        RuleFor(v => v.Username)
            .NotEmpty()
            .NotNull();

        RuleFor(v => v.Password)
            .NotEmpty()
            .NotNull();
    }
}