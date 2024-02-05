using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Contracts;
using Auctioneer.Application.Features.Members.Errors;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Auctions.Commands;

public class CreateAuctionController(ILogger<CreateAuctionController> logger) : ApiControllerBase(logger)
{
    [HttpPost("api/auction")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<Guid>> Create(CreateAuctionRequest request, CancellationToken cancellationToken)
    {
        using var _ = AuctioneerMetrics.MeasureRequestDuration();
        try
        {
            var command = new CreateAuctionCommand
            {
                MemberId = request.MemberId,
                Title = request.Title,
                Description = request.Description,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                StartingPrice = request.StartingPrice,
                ImgRoute = request.ImgRoute
            };

            var validationResult = await new CreateAuctionCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                return BadRequest(errorMessages);
            }

            var result = await Mediator.Send(command, cancellationToken);

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

public class CreateAuctionCommand : IRequest<Result<Guid>>
{
    public Guid MemberId { get; init; }
    public string MemberName { get; set; }
    public string Title { get; init; }
    public string Description { get; init; }
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public decimal StartingPrice { get; init; }
    public string ImgRoute { get; init; }
}

public class CreateAuctionCommandHandler(
    IRepository<Auction> auctionRepository,
    IRepository<Member> memberRepository,
    IRepository<DomainEvent> eventRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAuctionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await memberRepository.GetAsync(request.MemberId);

            if (member is null)
                return Result.Fail(new MemberNotFoundError());

            var auction = Auction.Create(
                request.MemberId,
                request.Title,
                request.Description,
                request.StartTime,
                request.EndTime,
                request.StartingPrice,
                request.ImgRoute
            );

            if (auction.IsFailed)
            {
                return Result.Fail(auction.Errors);
            }

            var domainEvent = new AuctionCreatedEvent(auction.Value, member, EventList.Auction.AuctionCreatedEvent);

            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await auctionRepository.CreateAsync(auction.Value, cancellationToken);
            await unitOfWork.SaveAsync();

            return Result.Ok(auction.Value.Id);
        }
        catch (Exception ex)
        {
            unitOfWork.CleanOperations();
            return Result.Fail(new Error(ex.Message));
        }
    }
}

public class CreateAuctionCommandValidator : AbstractValidator<CreateAuctionCommand>
{
    public CreateAuctionCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.Description)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.StartingPrice)
            .GreaterThan(-1);

        RuleFor(v => v.ImgRoute)
            .NotNull()
            .NotEmpty();
    }
}

public class AuctionCreatedEvent : DomainEvent, INotification
{
    public AuctionCreatedEvent(Auction auction, Member member, string @event)
    {
        Auction = auction;
        Member = member;
        Event = @event;
    }

    public Auction Auction { get; set; }
    public Member Member { get; set; }
}