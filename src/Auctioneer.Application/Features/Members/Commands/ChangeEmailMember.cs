using System.Reflection;
using Auctioneer.Application.Common;
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

internal sealed class ChangeEmailMemberCommandHandler : IRequestHandler<ChangeEmailMemberCommand, Result>
{
    private readonly IRepository<Member> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeEmailMemberCommandHandler(IRepository<Member> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangeEmailMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await _repository.GetAsync(request.MemberId);

            if (member is null)
                return Result.Fail(new Error("No member found"));

            var result = member.ChangeEmail(request.Email);

            if (!result.IsSuccess)
                return result;

            await _repository.UpdateAsync(member.Id, member);
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
            .NotNull();

        RuleFor(v => v.Email)
            .EmailAddress()
            .NotNull()
            .NotEmpty();
    }
}