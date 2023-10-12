using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.Commands;

public class ChangeEmailMemberController : ApiControllerBase
{
    private readonly ILogger<ChangeEmailMemberController> _logger;

    public ChangeEmailMemberController(ILogger<ChangeEmailMemberController> logger) : base(logger)
    {
        _logger = logger;
    }

    [HttpPut("api/member/change-email")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<Guid>> ChangeEmail(Guid memberId, string email)
    {
        try
        {
            var command = new ChangeEmailMemberCommand { MemberId = memberId, Email = email };

            var validationResult = await new ChangeEmailMemberCommandValidator().ValidateAsync(command);
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

public class ChangeEmailMemberCommand : IRequest<Result>
{
    public Guid MemberId { get; init; }
    public string Email { get; init; }
}

public class ChangeEmailMemberCommandHandler : IRequestHandler<ChangeEmailMemberCommand, Result>
{
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeEmailMemberCommandHandler(IRepository<Member> memberRepository,
        IRepository<DomainEvent> eventRepository, IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangeEmailMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await _memberRepository.GetAsync(request.MemberId);

            if (member is null)
                return Result.Fail(new Error("No member found"));

            var result = member.ChangeEmail(request.Email);

            if (!result.IsSuccess)
                return result;

            var domainEvent =
                new MemberChangedEmailEvent(member.Id, request.Email, EventList.Member.MemberChangedEmailEvent);

            await _memberRepository.UpdateAsync(member.Id, member);
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