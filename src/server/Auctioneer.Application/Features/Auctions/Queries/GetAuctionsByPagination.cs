using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Extensions;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Validators;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Contracts;
using Auctioneer.Application.Features.Auctions.Errors;
using Auctioneer.Application.Infrastructure.Persistence;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.Queries;

public class GetAuctionsByPaginationController(ILogger<GetAuctionsByPaginationController> logger)
    : ApiControllerBase(logger)
{
    [HttpGet("auctions/{pageNumber:int}/{pageSize:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetAuctionsByPaginationResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get([FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        using var _ = AuctioneerMetrics.MeasureRequestDuration();
        try
        {
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var validationResult = await new PaginationParamsValidator().ValidateAsync(paginationParams);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                return BadRequest(errorMessages);
            }

            var query = new GetAuctionsByPaginationQuery
            {
                PaginationParams = paginationParams
            };

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

public class GetAuctionsByPaginationQuery : IRequest<Result<GetAuctionsByPaginationResponse>>
{
    public PaginationParams PaginationParams { get; init; }
}

public class
    GetAuctionsByPaginationQueryHandler(IRepository<Auction> repository)
    : IRequestHandler<GetAuctionsByPaginationQuery, Result<GetAuctionsByPaginationResponse>>
{
    public async Task<Result<GetAuctionsByPaginationResponse>> Handle(GetAuctionsByPaginationQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var (totalPages, auctions) =
                await repository.GetAsync(request.PaginationParams.PageNumber, request.PaginationParams.PageSize);

            if (auctions is null || auctions.Count == 0)
                return Result.Fail(new AuctionNotFoundError());

            var response = new GetAuctionsByPaginationResponse
            {
                TotalPages = totalPages,
                PageNumber = request.PaginationParams.PageNumber,
                Auctions = auctions.ToDtos()
            };

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}