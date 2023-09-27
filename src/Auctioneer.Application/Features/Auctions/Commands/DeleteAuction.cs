using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.Commands;

public class DeleteAuctionController : ApiControllerBase
{
    private readonly ILogger<DeleteAuctionController> _logger;
    
    public DeleteAuctionController(ILogger<DeleteAuctionController> logger) : base(logger)
    {
        _logger = logger;
    }
    
    [HttpDelete("api/auction/{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<Guid>> Delete(Guid id)
    {
        try
        {
            var command = new DeleteAuctionCommand { AuctionId = id };
            
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                return Ok();
            
            return ReturnError(result.Errors.FirstOrDefault() as Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
        }
    }
}

public class DeleteAuctionCommand : IRequest<Result>
{
    public Guid AuctionId { get; init; }
}

internal sealed class DeleteAuctionCommandHandler : IRequestHandler<DeleteAuctionCommand, Result>
{
    private readonly IRepository<Auction> _repository;

    public DeleteAuctionCommandHandler(IRepository<Auction> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _repository.DeleteAsync(request.AuctionId);
            
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}