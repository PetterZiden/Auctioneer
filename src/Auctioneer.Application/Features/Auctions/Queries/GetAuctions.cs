using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Extensions;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Dto;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.Queries;

public class GetAuctionsController : ApiControllerBase
{
    private readonly ILogger<GetAuctionsController> _logger;
    
    public GetAuctionsController(ILogger<GetAuctionsController> logger) : base(logger)
    {
        _logger = logger;
    }
    
    [HttpGet("api/auctions")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<AuctionDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get()
    {
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
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
        }
    }
}

public class GetAuctionsQuery : IRequest<Result<List<AuctionDto>>> { }

internal sealed class GetAuctionsQueryHandler : IRequestHandler<GetAuctionsQuery, Result<List<AuctionDto>>>
{
    private readonly IRepository<Auction> _repository;

    public GetAuctionsQueryHandler(IRepository<Auction> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<AuctionDto>>> Handle(GetAuctionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var auctions = await _repository.GetAsync();

            if (!auctions?.Any() == true)
                return Result.Fail(new Error("No auctions found"));
            
            return Result.Ok(auctions.ToDtos());
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}