using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.Commands;

public class DeleteMemberController : ApiControllerBase
{
    private readonly ILogger<DeleteMemberController> _logger;

    public DeleteMemberController(ILogger<DeleteMemberController> logger) : base(logger)
    {
        _logger = logger;
    }

    [HttpDelete("api/member/{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<Guid>> Delete(Guid id)
    {
        try
        {
            var command = new DeleteMemberCommand { MemberId = id };

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

public class DeleteMemberCommand : IRequest<Result>
{
    public Guid MemberId { get; init; }
}

public class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand, Result>
{
    private readonly IRepository<Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMemberCommandHandler(IRepository<Member> memberRepository, IRepository<DomainEvent> eventRepository,
        IUnitOfWork unitOfWork)
    {
        _memberRepository = memberRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var domainEvent = new MemberDeletedEvent(request.MemberId, EventList.Member.MemberDeletedEvent);

            await _memberRepository.DeleteAsync(request.MemberId);
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

public class MemberDeletedEvent : DomainEvent, INotification
{
    public MemberDeletedEvent(Guid memberId, string @event)
    {
        DeletedMemberId = memberId;
        Event = @event;
    }

    public Guid DeletedMemberId { get; set; }
}