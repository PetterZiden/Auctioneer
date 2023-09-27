using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Models;
using Auctioneer.Application.Entities;
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
    
    [HttpPut("api/member/rate")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<Guid>> Rate(Guid memberId, Rating rating)
    {
        try
        {
            var command = new RateMemberCommand() { MemberId = memberId, Rating = rating};
            
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
    public Guid MemberId { get; init; }
    public Rating Rating { get; init; }
}

internal sealed class RateMemberCommandHandler : IRequestHandler<RateMemberCommand, Result>
{
    private readonly IRepository<Member> _repository;

    public RateMemberCommandHandler(IRepository<Member> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(RateMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await _repository.GetAsync(request.MemberId);

            if (member is null)
                return Result.Fail(new Error("No member found"));

            var result = member.Rate(request.Rating.RatingFromMemberId, request.Rating.Stars);

            if (!result.IsSuccess)
                return result;
            
            await _repository.UpdateAsync(member.Id, member);
            
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}

public class RateMemberCommandValidator : AbstractValidator<RateMemberCommand>
{
    public RateMemberCommandValidator()
    {
        RuleFor(v => v.MemberId)
            .NotNull();

        RuleFor(v => v.Rating.RatingFromMemberId)
            .NotNull();

        RuleFor(v => v.Rating.Stars)
            .GreaterThan(0)
            .LessThan(6);
    }
}