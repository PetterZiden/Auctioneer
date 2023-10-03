using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Dto;
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
    public async Task<ActionResult<Guid>> Update(MemberDto member)
    {
        try
        {
            var command = new UpdateMemberCommand { Member = member };

            var validationResult = await new UpdateMemberCommandValidator().ValidateAsync(command);
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

public class UpdateMemberCommand : IRequest<Result>
{
    public MemberDto Member { get; init; }
}

internal sealed class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, Result>
{
    private readonly IRepository<Member> _repository;

    public UpdateMemberCommandHandler(IRepository<Member> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Member.Id is null)
                return Result.Fail(new Error("Member id is required"));

            var member = await _repository.GetAsync(request.Member.Id.Value);

            if (member is null)
                return Result.Fail(new Error("No member found"));

            await _repository.UpdateAsync(request.Member.Id.Value, member);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}

public class UpdateMemberCommandValidator : AbstractValidator<UpdateMemberCommand>
{
    public UpdateMemberCommandValidator()
    {
        //Todo: fixa all validering
        RuleFor(v => v.Member.Id)
            .NotNull();

        RuleFor(v => v.Member.FirstName)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.Member.LastName)
            .NotNull()
            .NotEmpty();

        RuleFor(v => v.Member.Email)
            .EmailAddress()
            .NotNull()
            .NotEmpty();
    }
}