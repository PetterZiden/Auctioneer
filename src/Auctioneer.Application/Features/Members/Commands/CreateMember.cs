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
    public async Task<ActionResult<Guid>> Create(MemberDto member)
    {
        try
        {
            var command = new CreateMemberCommand { Member = member };
            
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
    public MemberDto Member { get; init; }
}

internal sealed class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Result<Guid>>
{
    private readonly IRepository<Member> _repository;

    public CreateMemberCommandHandler(IRepository<Member> repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = Member.Create(
                request.Member.FirstName,
                request.Member.LastName, 
                request.Member.Email,
                request.Member.PhoneNumber, 
                request.Member.Street, 
                request.Member.ZipCode, 
                request.Member.City
            );
            
            await _repository.CreateAsync(member);

            return Result.Ok(member.Id);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}

public class CreateMemberCommandValidator : AbstractValidator<CreateMemberCommand>
{
    public CreateMemberCommandValidator()
    {
        //Todo: fixa all validering
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