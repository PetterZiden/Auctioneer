using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Extensions;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Dto;
using Auctioneer.Application.Features.Auctions.Errors;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.Queries;

public class GetAuctionsController(ILogger<GetAuctionsController> logger) : ApiControllerBase(logger)
{
    [HttpGet("api/auctions")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<AuctionDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get()
    {
        using var _ = AuctioneerMetrics.MeasureRequestDuration();
        try
        {
            var query = new GetAuctionsQuery();

            var result = await Mediator.Send(query);

            if (result.IsSuccess)
                return Ok(result.Value);

            return ReturnError(result.Errors.FirstOrDefault() as Error);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
        }
        finally
        {
            AuctioneerMetrics.IncreaseAuctioneerRequestCount();
        }
    }
}

public class GetAuctionsQuery : IRequest<Result<List<AuctionDto>>>
{
}

public class GetAuctionsQueryHandler(IRepository<Auction> repository)
    : IRequestHandler<GetAuctionsQuery, Result<List<AuctionDto>>>
{
    public async Task<Result<List<AuctionDto>>> Handle(GetAuctionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var auctions = await repository.GetAsync();

            if (auctions is null || auctions.Count == 0)
                return Result.Fail(new AuctionNotFoundError());

            return Result.Ok(auctions.ToDtos());
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}