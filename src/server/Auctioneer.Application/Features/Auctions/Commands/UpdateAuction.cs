using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Contracts;
using Auctioneer.Application.Features.Auctions.Errors;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.Commands;

public class UpdateAuctionController : ApiControllerBase
{
    private readonly ILogger<UpdateAuctionController> _logger;

    public UpdateAuctionController(ILogger<UpdateAuctionController> logger) : base(logger)
    {
        _logger = logger;
    }

    [HttpPut("api/auction")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Update(UpdateAuctionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateAuctionCommand
            {
                Id = request.AuctionId,
                Title = request.Title,
                Description = request.Description,
                ImgRoute = request.ImgRoute
            };

            var validationResult = await new UpdateAuctionCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                return BadRequest(errorMessages);
            }

            var result = await Mediator.Send(command, cancellationToken: cancellationToken);

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

public class UpdateAuctionCommand : IRequest<Result>
{
    public Guid Id { get; init; }
#nullable enable
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? ImgRoute { get; init; }
#nullable disable
}

public class UpdateAuctionCommandHandler : IRequestHandler<UpdateAuctionCommand, Result>
{
    private readonly IRepository<Auction> _auctionRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAuctionCommandHandler(IRepository<Auction> auctionRepository, IRepository<DomainEvent> eventRepository,
        IUnitOfWork unitOfWork)
    {
        _auctionRepository = auctionRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await _auctionRepository.GetAsync(request.Id);

            if (auction is null)
                return Result.Fail(new AuctionNotFoundError());


            if (!string.IsNullOrEmpty(request.Title))
            {
                var result = auction.ChangeTitle(request.Title);
                if (!result.IsSuccess)
                    return result;
            }

            if (!string.IsNullOrEmpty(request.Description))
            {
                var result = auction.ChangeDescription(request.Description);
                if (!result.IsSuccess)
                    return result;
            }

            if (!string.IsNullOrEmpty(request.ImgRoute))
            {
                var result = auction.ChangeImageRoute(request.ImgRoute);
                if (!result.IsSuccess)
                    return result;
            }

            var domainEvent = new AuctionUpdatedEvent(auction, EventList.Auction.AuctionUpdatedEvent);

            await _eventRepository.CreateAsync(domainEvent, cancellationToken);
            await _auctionRepository.UpdateAsync(request.Id, auction, cancellationToken);
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

public class UpdateAuctionCommandValidator : AbstractValidator<UpdateAuctionCommand>
{
    public UpdateAuctionCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty()
            .NotNull();

        RuleFor(v => v.Title)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.Description)
            .NotNull()
            .NotEmpty();
    }
}

public class AuctionUpdatedEvent : DomainEvent, INotification
{
    public AuctionUpdatedEvent(Auction auction, string @event)
    {
        Auction = auction;
        Event = @event;
    }

    public Auction Auction { get; set; }
}