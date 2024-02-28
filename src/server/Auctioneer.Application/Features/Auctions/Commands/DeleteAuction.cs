using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Errors;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.Commands;

public class DeleteAuctionController(ILogger<DeleteAuctionController> logger) : ApiControllerBase(logger)
{
    [HttpDelete("auction/{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        using var _ = AuctioneerMetrics.MeasureRequestDuration();
        try
        {
            var command = new DeleteAuctionCommand { AuctionId = id };

            var result = await Mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
                return Ok();

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

public class DeleteAuctionCommand : IRequest<Result>
{
    public Guid AuctionId { get; init; }
}

public class DeleteAuctionCommandHandler(
    IRepository<Auction> auctionRepository,
    IRepository<DomainEvent> eventRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAuctionCommand, Result>
{
    public async Task<Result> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await auctionRepository.GetAsync(request.AuctionId);
            if (auction is null)
                return Result.Fail(new AuctionNotFoundError());

            var domainEvent = new AuctionDeletedEvent(request.AuctionId, EventList.Auction.AuctionDeletedEvent);

            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await auctionRepository.DeleteAsync(request.AuctionId, cancellationToken);
            await unitOfWork.SaveAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            unitOfWork.CleanOperations();
            return Result.Fail(new Error(ex.Message));
        }
    }
}

public class AuctionDeletedEvent : DomainEvent, INotification
{
    public AuctionDeletedEvent(Guid auctionId, string @event)
    {
        DeletedAuctionId = auctionId;
        Event = @event;
    }

    public Guid DeletedAuctionId { get; set; }
}