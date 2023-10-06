using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Entities;
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
    public async Task<ActionResult<Guid>> Rate(Rating rating)
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

internal sealed class RateMemberCommandHandler : IRequestHandler<RateMemberCommand, Result>
{
    private readonly IRepository<Member> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageProducer _messageProducer;
    private readonly INotificationProducer _notificationProducer;

    public RateMemberCommandHandler(IRepository<Member> repository, IMessageProducer messageProducer,
        INotificationProducer notificationProducer, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _messageProducer = messageProducer;
        _notificationProducer = notificationProducer;
    }

    public async Task<Result> Handle(RateMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ratedMember = await _repository.GetAsync(request.Rating.RatingForMemberId);
            var ratedByMember = await _repository.GetAsync(request.Rating.RatingFromMemberId);

            if (ratedMember is null || ratedByMember is null)
                return Result.Fail(new Error("No member found"));

            var result = ratedMember.Rate(request.Rating.RatingFromMemberId, request.Rating.Stars);

            if (!result.IsSuccess)
                return result;

            await _repository.UpdateAsync(ratedMember.Id, ratedMember);
            await _unitOfWork.SaveAsync();

            _messageProducer.PublishMessage(new Message<RateMemberMessage>
            {
                Queue = RabbitMqSettings.RateMemberQueue,
                Exchange = RabbitMqSettings.AuctionExchange,
                ExchangeType = RabbitMqSettings.AuctionExchangeType,
                RouteKey = RabbitMqSettings.RateMemberRouteKey,
                Data = new RateMemberMessage(
                    ratedMember.FullName,
                    ratedMember.Email,
                    ratedByMember.FullName,
                    request.Rating.Stars)
            });

            _notificationProducer.PublishNotification(new Notification<RateMemberNotification>
            {
                Data = new RateMemberNotification(
                    ratedMember.Id,
                    ratedByMember.FullName,
                    request.Rating.Stars)
            });

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
            .NotNull();

        RuleFor(v => v.Rating.RatingFromMemberId)
            .NotNull();

        RuleFor(v => v.Rating.Stars)
            .GreaterThan(0)
            .LessThan(6);
    }
}