using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Extensions;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Dto;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.Queries;

public class GetMembersController : ApiControllerBase
{
    private readonly ILogger<GetMembersController> _logger;
    
    public GetMembersController(ILogger<GetMembersController> logger) : base(logger)
    {
        _logger = logger;
    }
    
    [HttpGet("api/members")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<MemberDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get()
    {
        try
        {
            var query = new GetMembersQuery();
            
            var result = await Mediator.Send(query);

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

public class GetMembersQuery : IRequest<Result<List<MemberDto>>> { }

internal sealed class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, Result<List<MemberDto>>>
{
    private readonly IRepository<Member> _repository;

    public GetMembersQueryHandler(IRepository<Member> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<MemberDto>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var members = await _repository.GetAsync();

            if (!members?.Any() == true)
                return Result.Fail(new Error("No members found"));
            
            return Result.Ok(members.ToDtos());
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}