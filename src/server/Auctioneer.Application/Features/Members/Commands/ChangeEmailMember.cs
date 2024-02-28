using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Contracts;
using Auctioneer.Application.Features.Members.Errors;
using Auctioneer.Application.ValueObjects;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.Commands;

public class ChangeEmailMemberController(ILogger<ChangeEmailMemberController> logger) : ApiControllerBase(logger)
{
    [HttpPut("member/change-email")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> ChangeEmail(ChangeMemberEmailRequest request, CancellationToken cancellationToken)
    {
        using var _ = AuctioneerMetrics.MeasureRequestDuration();
        try
        {
            var command = new ChangeEmailMemberCommand { MemberId = request.MemberId, Email = request.Email };

            var validationResult =
                await new ChangeEmailMemberCommandValidator().ValidateAsync(command, cancellationToken);
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

public class ChangeEmailMemberCommand : IRequest<Result>
{
    public Guid MemberId { get; init; }
    public string Email { get; init; }
}

public class ChangeEmailMemberCommandHandler(
    IRepository<Member> memberRepository,
    IRepository<DomainEvent> eventRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeEmailMemberCommand, Result>
{
    public async Task<Result> Handle(ChangeEmailMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await memberRepository.GetAsync(request.MemberId);

            if (member is null)
                return Result.Fail(new MemberNotFoundError());

            var result = member.ChangeEmail(new Email(request.Email));

            if (!result.IsSuccess)
                return result;

            var domainEvent =
                new MemberChangedEmailEvent(member.Id, request.Email, EventList.Member.MemberChangedEmailEvent);

            await memberRepository.UpdateAsync(member.Id, member, cancellationToken);
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

public class ChangeEmailMemberCommandValidator : AbstractValidator<ChangeEmailMemberCommand>
{
    public ChangeEmailMemberCommandValidator()
    {
        RuleFor(v => v.MemberId)
            .NotEmpty()
            .NotNull();

        RuleFor(v => v.Email)
            .EmailAddress()
            .NotNull()
            .NotEmpty();
    }
}

public class MemberChangedEmailEvent : DomainEvent, INotification
{
    public MemberChangedEmailEvent(Guid memberId, string email, string @event)
    {
        MemberId = memberId;
        Email = email;
        Event = @event;
    }

    public Guid MemberId { get; set; }
    public string Email { get; set; }
}