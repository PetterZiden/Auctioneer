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

public class UpdateAuctionController(ILogger<UpdateAuctionController> logger) : ApiControllerBase(logger)
{
    [HttpPut("api/auction")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Update(UpdateAuctionRequest request, CancellationToken cancellationToken)
    {
        using var _ = AuctioneerMetrics.MeasureRequestDuration();
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
            logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
        }
        finally
        {
            AuctioneerMetrics.IncreaseAuctioneerRequestCount();
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

public class UpdateAuctionCommandHandler(
    IRepository<Auction> auctionRepository,
    IRepository<DomainEvent> eventRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAuctionCommand, Result>
{
    public async Task<Result> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var auction = await auctionRepository.GetAsync(request.Id);

            if (auction is null)
                return Result.Fail(new AuctionNotFoundError());


            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                var result = auction.ChangeTitle(request.Title);
                if (!result.IsSuccess)
                    return result;
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                var result = auction.ChangeDescription(request.Description);
                if (!result.IsSuccess)
                    return result;
            }

            if (!string.IsNullOrWhiteSpace(request.ImgRoute))
            {
                var result = auction.ChangeImageRoute(request.ImgRoute);
                if (!result.IsSuccess)
                    return result;
            }

            var domainEvent = new AuctionUpdatedEvent(auction, EventList.Auction.AuctionUpdatedEvent);

            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await auctionRepository.UpdateAsync(request.Id, auction, cancellationToken);
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