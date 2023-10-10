using System.Reflection;
using System.Text.Json;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Contracts;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.Commands;

public class CreateMemberController : ApiControllerBase
{
    private readonly ILogger<CreateMemberController> _logger;

    public CreateMemberController(ILogger<CreateMemberController> logger) : base(logger)
    {
        _logger = logger;
    }

    [HttpPost("api/member")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<Guid>> Create(CreateMemberRequest request)
    {
        try
        {
            var command = new CreateMemberCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Street = request.Street,
                ZipCode = request.ZipCode,
                City = request.City,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            var validationResult = await new CreateMemberCommandValidator().ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                return BadRequest(errorMessages);
            }

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                return Ok(result.Value);

            return ReturnError(result.Errors.FirstOrDefault() as Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
        }
    }
}

public class CreateMemberCommand : IRequest<Result<Guid>>
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Street { get; init; }
    public string ZipCode { get; init; }
    public string City { get; init; }
    public string Email { get; init; }
    public string PhoneNumber { get; init; }
}

internal sealed class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Result<Guid>>
{
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMemberCommandHandler(IRepository<Member> memberRepository, IUnitOfWork unitOfWork,
        IRepository<DomainEvent> eventRepository)
    {
        _memberRepository = memberRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = Member.Create(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.Street,
                request.ZipCode,
                request.City
            );

            var domainEvent = new MemberCreatedEvent(member, "member.created");

            await _memberRepository.CreateAsync(member);
            await _eventRepository.CreateAsync(domainEvent);
            await _unitOfWork.SaveAsync();

            return Result.Ok(member.Id);
        }
        catch (Exception ex)
        {
            _unitOfWork.CleanOperations();
            return Result.Fail(new Error(ex.Message));
        }
    }
}

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        //Todo: fixa all validering
        RuleFor(v => v.FirstName)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.LastName)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.Email)
            .EmailAddress()
            .NotNull()
            .NotEmpty();
    }
}

public class MemberCreatedEvent : DomainEvent, INotification
{
    public MemberCreatedEvent(Member member, string @event)
    {
        Member = member;
        Event = @event;
    }

    public Member Member { get; set; }
}