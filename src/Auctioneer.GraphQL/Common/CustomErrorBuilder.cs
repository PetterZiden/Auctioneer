using FluentResults;
using FluentValidation.Results;
using IError = HotChocolate.IError;

namespace Auctioneer.GraphQL.Common;

public abstract class CustomErrorBuilder
{
    public static IError CreateError(ValidationResult validationError)
    {
        return ErrorBuilder.New()
            .SetMessage(string.Join(Environment.NewLine, validationError.Errors.ConvertAll(e => e.ErrorMessage)))
            .SetCode("BAD_REQUEST")
            .Build();
    }

    public static IError CreateError(List<FluentResults.IError> errors)
    {
        return ErrorBuilder.New()
            .SetMessage(string.Join(Environment.NewLine, errors.ConvertAll(e => e.Message)))
            .SetCode("INTERNAL_ERROR")
            .Build();
    }

    public static IError CreateError(FluentResults.IError error)
    {
        return ErrorBuilder.New()
            .SetMessage(error.Message)
            .SetCode(GetStatusCode(error))
            .Build();
    }

    private static string GetStatusCode(IReason error)
    {
        if (error.Metadata.TryGetValue("HttpStatusCode", out var httpStatusCode))
        {
            return httpStatusCode.ToString() switch
            {
                "400" => "BAD_REQUEST",
                "404" => "NOT_FOUND",
                _ => "INTERNAL_ERROR"
            };
        }

        return "INTERNAL_ERROR";
    }
}