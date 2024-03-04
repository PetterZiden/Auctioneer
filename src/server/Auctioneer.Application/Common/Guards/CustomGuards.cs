using Ardalis.GuardClauses;
using Auctioneer.Application.ValueObjects;

namespace Auctioneer.Application.Common.Guards;

public static class CustomGuards
{
    public static DateTimeOffset DateTimeOffsetLesserThan(this IGuardClause guardClause, DateTimeOffset input,
        DateTimeOffset minDate, string? parameterName = null, string? message = null)
    {
        if (input < minDate)
        {
            throw new ArgumentException(message ?? $"Required input {parameterName} can not be lesser than {minDate}.",
                parameterName);
        }

        return input;
    }

    public static decimal NegativeOrZeroOrGreaterThan(this IGuardClause guardClause, decimal input, decimal lesserThan,
        string? parameterName = null, string? message = null)
    {
        if (input <= 0)
        {
            throw new ArgumentException($"Required input {parameterName} cannot be zero or negative.", parameterName);
        }

        if (input < lesserThan)
        {
            throw new ArgumentException(
                message ?? $"Required input {parameterName} can not be greater than {lesserThan}.", parameterName);
        }

        return input;
    }

    public static string IsNullOrWhitespaceOrSameAsCurrent(this IGuardClause guardClause, string input, string current,
        string? parameterName = null, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException($"Required input {parameterName} was empty.", parameterName);
        }

        if (input.Equals(current))
        {
            throw new ArgumentException(message ?? $"Required input {parameterName} can not be same as current value.",
                parameterName);
        }

        return input;
    }

    public static Email IsNullOrWhitespaceOrSameAsCurrent(this IGuardClause guardClause, Email input, Email current,
        string? parameterName = null, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(input.Value))
        {
            throw new ArgumentException($"Required input {parameterName} was empty.", parameterName);
        }

        if (input.Equals(current))
        {
            throw new ArgumentException(message ?? $"Required input {parameterName} can not be same as current value.",
                parameterName);
        }

        return input;
    }

    public static Address IsSameAsCurrent(this IGuardClause guardClause, Address input, Address current,
        string? parameterName = null, string? message = null)
    {
        if (input.Equals(current))
        {
            throw new ArgumentException(message ?? $"Required input {parameterName} can not be same as current value.",
                parameterName);
        }

        return input;
    }

    public static string IsNullOrWhitespaceOrInvalidEmail(this IGuardClause guardClause, string input,
        string? parameterName = null, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException($"Required input {parameterName} was empty.", parameterName);
        }

        var trimmedEmail = input.Trim();

        if (trimmedEmail.EndsWith('.'))
        {
            throw new ArgumentException(message ?? $"Required input {parameterName} can not be same as current value.",
                parameterName);
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(input);
            if (addr.Address.Equals(trimmedEmail))
            {
                return input;
            }

            throw new ArgumentException(message ?? $"Required input {parameterName} can not be same as current value.",
                parameterName);
        }
        catch
        {
            throw new ArgumentException(message ?? $"Required input {parameterName} can not be same as current value.",
                parameterName);
        }
    }
}