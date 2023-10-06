using System.Reflection;
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
    public async Task<ActionResult<Guid>> Update(UpdateMemberRequest request)
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
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

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
    public Guid Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Street { get; init; }
    public string ZipCode { get; init; }
    public string City { get; init; }
    public string Email { get; init; }
    public string PhoneNumber { get; init; }
}

internal sealed class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, Result>
{
    private readonly IRepository<Member> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMemberCommandHandler(IRepository<Member> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await _repository.GetAsync(request.Id);

            if (member is null)
                return Result.Fail(new Error("No member found"));

            await _repository.UpdateAsync(request.Id, member);
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
            .NotNull();

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