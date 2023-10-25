using System.ComponentModel.DataAnnotations;
using Auctioneer.Application.Common;

namespace Auctioneer.Application.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }

    public Email(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException(nameof(email));

        if (!IsValidEmail(email))
            throw new ValidationException($"{nameof(email)} is not a valid email");

        Value = email;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    private static bool IsValidEmail(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith("."))
        {
            return false;
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }
    }
}