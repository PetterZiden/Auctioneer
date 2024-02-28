using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
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

public class UpdateMemberController(ILogger<UpdateMemberController> logger) : ApiControllerBase(logger)
{
    [HttpPut("member")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Update(UpdateMemberRequest request, CancellationToken cancellationToken)
    {
        using var _ = AuctioneerMetrics.MeasureRequestDuration();
        try
        {
            var command = new UpdateMemberCommand
            {
                Id = request.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Street = request.Street,
                Zipcode = request.ZipCode,
                City = request.City,
                PhoneNumber = request.PhoneNumber
            };

            var validationResult = await new UpdateMemberCommandValidator().ValidateAsync(command, cancellationToken);
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

public class UpdateMemberCommand : IRequest<Result>
{
    public Guid Id { get; init; }
#nullable enable
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Street { get; init; }
    public string? Zipcode { get; init; }
    public string? City { get; init; }
    public string? PhoneNumber { get; init; }
#nullable disable
}

public class UpdateMemberCommandHandler(
    IRepository<Member> memberRepository,
    IRepository<DomainEvent> eventRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMemberCommand, Result>
{
    public async Task<Result> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await memberRepository.GetAsync(request.Id);

            if (member is null)
                return Result.Fail(new MemberNotFoundError());

            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                var result = member.ChangeFirstName(request.FirstName);
                if (!result.IsSuccess)
                    return result;
            }

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                var result = member.ChangeLastName(request.LastName);
                if (!result.IsSuccess)
                    return result;
            }

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var result = member.ChangePhoneNumber(request.PhoneNumber);
                if (!result.IsSuccess)
                    return result;
            }

            if (!string.IsNullOrWhiteSpace(request.Street) || !string.IsNullOrWhiteSpace(request.Zipcode) ||
                !string.IsNullOrWhiteSpace(request.City))
            {
                var addressToUpdate = new Address(request.Street ?? member.Address.Street,
                    request.Zipcode ?? member.Address.Zipcode, request.City ?? member.Address.City);

                var result = member.ChangeAddress(addressToUpdate);
                if (!result.IsSuccess)
                    return result;
            }

            var domainEvent = new MemberUpdatedEvent(member, EventList.Member.MemberUpdatedEvent);

            await eventRepository.CreateAsync(domainEvent, cancellationToken);
            await memberRepository.UpdateAsync(request.Id, member, cancellationToken);
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

public class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty()
            .NotNull();
    }
}

public class MemberUpdatedEvent : DomainEvent, INotification
{
    public MemberUpdatedEvent(Member member, string @event)
    {
        Member = member;
        Event = @event;
    }

    public Member Member { get; set; }
}