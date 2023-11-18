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
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
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
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
        }
    }
}

public class DeleteAuctionCommand : IRequest<Result>
{
    public Guid AuctionId { get; init; }
}

public class DeleteAuctionCommandHandler : IRequestHandler<DeleteAuctionCommand, Result>
{
    private readonly IRepository<Auction> _auctionRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAuctionCommandHandler(IRepository<Auction> auctionRepository, IRepository<DomainEvent> eventRepository,
        IUnitOfWork unitOfWork)
    {
        _auctionRepository = auctionRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteAuctionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await _auctionRepository.GetAsync(request.AuctionId);
            if (auction is null)
                return Result.Fail(new AuctionNotFoundError());

            var domainEvent = new AuctionDeletedEvent(request.AuctionId, EventList.Auction.AuctionDeletedEvent);

            await _eventRepository.CreateAsync(domainEvent, cancellationToken);
            await _auctionRepository.DeleteAsync(request.AuctionId, cancellationToken);
            await _unitOfWork.SaveAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _unitOfWork.CleanOperations();
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