using Auctioneer.Application.Auth.Models;
using FluentValidation;

namespace Auctioneer.Application.Common.Validators;

public class RegisterUserValidator : AbstractValidator<RegisterUser>
{
    public RegisterUserValidator()
    {
        RuleFor(v => v.Username)
            .NotEmpty()
            .NotNull();

        RuleFor(v => v.Email)
            .EmailAddress()
            .NotEmpty()
            .NotNull();

        RuleFor(v => v.Password)
            .NotEmpty()
            .NotNull();
    }
}