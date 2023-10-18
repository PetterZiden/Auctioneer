using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Contracts;
using Auctioneer.Application.Features.Members.Errors;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.Commands;

public class UpdateMemberController : ApiControllerBase
{
    private readonly ILogger<UpdateMemberController> _logger;

    public UpdateMemberController(ILogger<UpdateMemberController> logger) : base(logger)
    {
        _logger = logger;
    }

    [HttpPut("api/member")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Update(UpdateMemberRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateMemberCommand
            {
                Id = request.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Street = request.Street,
                ZipCode = request.ZipCode,
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
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
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
    public string? ZipCode { get; init; }
    public string? City { get; init; }
    public string? PhoneNumber { get; init; }
#nullable disable
}

public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, Result>
{
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMemberCommandHandler(IRepository<Member> memberRepository, IRepository<DomainEvent> eventRepository,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await _memberRepository.GetAsync(request.Id);

            if (member is null)
                return Result.Fail(new MemberNotFoundError());

            if (!string.IsNullOrEmpty(request.FirstName))
            {
                var result = member.ChangeFirstName(request.FirstName);
                if (!result.IsSuccess)
                    return result;
            }

            if (!string.IsNullOrEmpty(request.LastName))
            {
                var result = member.ChangeLastName(request.LastName);
                if (!result.IsSuccess)
                    return result;
            }

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                var result = member.ChangePhoneNumber(request.PhoneNumber);
                if (!result.IsSuccess)
                    return result;
            }

            if (!string.IsNullOrEmpty(request.Street) || !string.IsNullOrEmpty(request.ZipCode) ||
                !string.IsNullOrEmpty(request.City))
            {
                var addressToUpdate = new Address
                {
                    Street = request.Street ?? member.Address.Street,
                    ZipCode = request.ZipCode ?? member.Address.ZipCode,
                    City = request.City ?? member.Address.City
                };
                var result = member.ChangeAddress(addressToUpdate);
                if (!result.IsSuccess)
                    return result;
            }

            var domainEvent = new MemberUpdatedEvent(member, EventList.Member.MemberUpdatedEvent);

            await _eventRepository.CreateAsync(domainEvent, cancellationToken);
            await _memberRepository.UpdateAsync(request.Id, member, cancellationToken);
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

public class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        //Todo: fixa all validering
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