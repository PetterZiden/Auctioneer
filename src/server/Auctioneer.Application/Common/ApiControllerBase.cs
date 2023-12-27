using Auctioneer.Application.Common.Metrics;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Common;

[Authorize]
[ApiController]
public abstract class ApiControllerBase(ILogger logger) : ControllerBase
{
    private ISender? _mediator;
    private AuctioneerMetrics _auctioneerMetrics;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetService<ISender>()!;

    protected AuctioneerMetrics AuctioneerMetrics =>
        _auctioneerMetrics ??= HttpContext.RequestServices.GetService<AuctioneerMetrics>();

    protected ObjectResult ReturnError(Error error)
    {
        var errorMessage = error.Message;
        if (error.Metadata.TryGetValue("HttpStatusCode", out var httpStatusCode))
        {
            logger.LogError(errorMessage);
            return httpStatusCode.ToString() switch
            {
                "400" => BadRequest(errorMessage),
                "404" => NotFound(errorMessage),
                _ => StatusCode(500, errorMessage)
            };
        }

        logger.LogError("Could not find HttpStatusCode in Error-Metadata");
        return StatusCode(500, errorMessage);
    }
}