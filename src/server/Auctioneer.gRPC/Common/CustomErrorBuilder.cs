using FluentResults;
using FluentValidation.Results;
using Grpc.Core;

namespace Auctioneer.gRPC.Common;

public abstract class CustomErrorBuilder
{
    public static Status CreateError(ValidationResult validationError)
    {
        return new Status(StatusCode.InvalidArgument,
            string.Join(Environment.NewLine, validationError.Errors.ConvertAll(e => e.ErrorMessage)));
    }

    public static Status CreateError(List<IError> errors)
    {
        return new Status(StatusCode.Internal,
            string.Join(Environment.NewLine, errors.ConvertAll(e => e.Message)));
    }

    public static Status CreateError(IError error)
    {
        return new Status(GetStatusCode(error), error.Message);
    }

    private static StatusCode GetStatusCode(IReason error)
    {
        if (error.Metadata.TryGetValue("HttpStatusCode", out var httpStatusCode))
        {
            return httpStatusCode.ToString() switch
            {
                "400" => StatusCode.InvalidArgument,
                "404" => StatusCode.NotFound,
                _ => StatusCode.Internal
            };
        }

        return StatusCode.Internal;
    }
}