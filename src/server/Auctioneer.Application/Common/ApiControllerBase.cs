using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Common;

//[Authorize]
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    private readonly ILogger _logger;

    protected ApiControllerBase(ILogger logger)
    {
        _logger = logger;
    }

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetService<ISender>()!;

    protected ObjectResult ReturnError(Error error)
    {
        var errorMessage = error.Message;
        if (error.Metadata.TryGetValue("HttpStatusCode", out var httpStatusCode))
        {
            _logger.LogError(errorMessage);
            return httpStatusCode.ToString() switch
            {
                "400" => BadRequest(errorMessage),
                "404" => NotFound(errorMessage),
                _ => StatusCode(500, errorMessage)
            };
        }

        _logger.LogError("Could not find HttpStatusCode in Error-Metadata");
        return StatusCode(500, errorMessage);
    }
}