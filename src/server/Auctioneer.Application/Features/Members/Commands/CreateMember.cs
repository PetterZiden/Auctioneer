using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Contracts;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.Commands;

public class CreateMemberController(ILogger<CreateMemberController> logger) : ApiControllerBase(logger)
{
    [HttpPost("api/member")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<Guid>> Create(CreateMemberRequest request, CancellationToken cancellationToken)
    {
        using var _ = AuctioneerMetrics.MeasureRequestDuration();
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

            var validationResult = await new CreateMemberCommandValidator().ValidateAsync(command, cancellationToken);
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

public class CreateMemberCommandHandler(
    IRepository<Member> memberRepository,
    IRepository<DomainEvent> eventRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateMemberCommand, Result<Guid>>
{
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

            if (member.IsFailed)
            {
                return Result.Fail(member.Errors);
            }

            var domainEvent = new MemberCreatedEvent(member.Value, EventList.Member.MemberCreatedEvent);

            await memberRepository.CreateAsync(member.Value, cancellationToken);
            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await unitOfWork.SaveAsync();

            return Result.Ok(member.Value.Id);
        }
        catch (Exception ex)
        {
            unitOfWork.CleanOperations();
            return Result.Fail(new Error(ex.Message));
        }
    }
}

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
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

        RuleFor(v => v.PhoneNumber)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.Street)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.ZipCode)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.City)
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