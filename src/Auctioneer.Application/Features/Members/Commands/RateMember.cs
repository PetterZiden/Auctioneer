using System.Reflection;
using System.Text.Json;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Dto;
using Auctioneer.Application.Infrastructure.Messaging.MassTransit;
using Auctioneer.Application.Infrastructure.Messaging.RabbitMq;
using Auctioneer.MessagingContracts.Email;
using Auctioneer.MessagingContracts.Notification;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.Commands;

public class RateMemberController : ApiControllerBase
{
    private readonly ILogger<RateMemberController> _logger;

    public RateMemberController(ILogger<RateMemberController> logger) : base(logger)
    {
        _logger = logger;
    }

    [HttpPost("api/member/rate")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Rate(Rating rating)
    {
        try
        {
            var command = new RateMemberCommand { Rating = rating };

            var validationResult = await new RateMemberCommandValidator().ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                return BadRequest(errorMessages);
            }

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

public class RateMemberCommand : IRequest<Result>
{
    public Rating Rating { get; init; }
}

public class RateMemberCommandHandler : IRequestHandler<RateMemberCommand, Result>
{
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RateMemberCommandHandler(IRepository<Member> memberRepository, IRepository<DomainEvent> eventRepository,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RateMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ratedMember = await _memberRepository.GetAsync(request.Rating.RatingForMemberId);
            var ratedByMember = await _memberRepository.GetAsync(request.Rating.RatingFromMemberId);

            if (ratedMember is null || ratedByMember is null)
                return Result.Fail(new Error("No member found"));

            var result = ratedMember.Rate(request.Rating.RatingFromMemberId, request.Rating.Stars);

            if (!result.IsSuccess)
                return result;

            var rateMemberDto = new RateMemberDto
            {
                RatedName = ratedMember.FullName,
                RatedMemberId = ratedMember.Id,
                RatedEmail = ratedMember.Email,
                RatedByName = ratedByMember.FullName,
                Stars = request.Rating.Stars
            };
            var domainEvent = new RateMemberEvent(rateMemberDto, EventList.Member.RateMemberEvent);

            await _memberRepository.UpdateAsync(ratedMember.Id, ratedMember);
            await _eventRepository.CreateAsync(domainEvent);
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

public class RateMemberCommandValidator : AbstractValidator<RateMemberCommand>
{
    public RateMemberCommandValidator()
    {
        RuleFor(v => v.Rating.RatingForMemberId)
            .NotEmpty()
            .NotNull();

        RuleFor(v => v.Rating.RatingFromMemberId)
            .NotEmpty()
            .NotNull();

        RuleFor(v => v.Rating.Stars)
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