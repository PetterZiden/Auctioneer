using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Dto;
using Auctioneer.Application.Features.Members.Errors;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement.Mvc;

namespace Auctioneer.Application.Features.Members.Commands;

public class RateMemberController(ILogger<RateMemberController> logger) : ApiControllerBase(logger)
{
    [HttpPost("member/rate")]
    [FeatureGate("RateMemberPercentage")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Rate(Rating request, CancellationToken cancellationToken)
    {
        using var _ = AuctioneerMetrics.MeasureRequestDuration();
        try
        {
            var command = new RateMemberCommand
            {
                RatingForMemberId = request.RatingForMemberId,
                RatingFromMemberId = request.RatingFromMemberId,
                Stars = request.Stars
            };

            var validationResult = await new RateMemberCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                return BadRequest(errorMessages);
            }

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

public class RateMemberCommand : IRequest<Result>
{
    public Guid RatingForMemberId { get; init; }
    public Guid RatingFromMemberId { get; init; }
    public int Stars { get; init; }
}

public class RateMemberCommandHandler(
    IRepository<Member> memberRepository,
    IRepository<DomainEvent> eventRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RateMemberCommand, Result>
{
    public async Task<Result> Handle(RateMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ratedMember = await memberRepository.GetAsync(request.RatingForMemberId);
            var ratedByMember = await memberRepository.GetAsync(request.RatingFromMemberId);

            if (ratedMember is null || ratedByMember is null)
                return Result.Fail(new MemberNotFoundError());

            var result = ratedMember.Rate(request.RatingFromMemberId, request.Stars);

            if (!result.IsSuccess)
                return result;

            var rateMemberDto = new RateMemberDto
            {
                RatedName = ratedMember.FullName,
                RatedMemberId = ratedMember.Id,
                RatedEmail = ratedMember.Email.Value,
                RatedByName = ratedByMember.FullName,
                Stars = request.Stars
            };
            var domainEvent = new RateMemberEvent(rateMemberDto, EventList.Member.RateMemberEvent);

            await memberRepository.UpdateAsync(ratedMember.Id, ratedMember, cancellationToken);
            await eventRepository.CreateAsync(domainEvent, cancellationToken);
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

public class RateMemberCommandValidator : AbstractValidator<RateMemberCommand>
{
    public RateMemberCommandValidator()
    {
        RuleFor(v => v.RatingForMemberId)
            .NotEmpty()
            .NotNull();

        RuleFor(v => v.RatingFromMemberId)
            .NotEmpty()
            .NotNull();

        RuleFor(v => v.Stars)
            .GreaterThan(0)
            .LessThan(6);
    }
}

public class RateMemberEvent : DomainEvent, INotification
{
    public RateMemberEvent(RateMemberDto rateMember, string @event)
    {
        RateMember = rateMember;
        Event = @event;
    }

    public RateMemberDto RateMember { get; set; }
}