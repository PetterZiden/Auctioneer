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

public class GetAuctionController(ILogger<GetAuctionController> logger) : ApiControllerBase(logger)
{
    [HttpGet("api/auction/{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AuctionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get(Guid id)
    {
        using var _ = AuctioneerMetrics.MeasureRequestDuration();
        try
        {
            var query = new GetAuctionQuery { Id = id };

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

public class GetAuctionQuery : IRequest<Result<AuctionDto>>
{
    public Guid Id { get; init; }
}

public class GetAuctionQueryHandler(IRepository<Auction> repository)
    : IRequestHandler<GetAuctionQuery, Result<AuctionDto>>
{
    public async Task<Result<AuctionDto>> Handle(GetAuctionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await repository.GetAsync(request.Id);

            if (auction is null)
                return Result.Fail(new AuctionNotFoundError());

            return Result.Ok(auction.ToDto());
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}