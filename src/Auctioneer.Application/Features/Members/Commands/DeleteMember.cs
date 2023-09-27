using System.Reflection;
using Auctioneer.Application.Common;
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

internal sealed class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand, Result>
{
    private readonly IRepository<Member> _repository;

    public DeleteMemberCommandHandler(IRepository<Member> repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _repository.DeleteAsync(request.MemberId);
            
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}