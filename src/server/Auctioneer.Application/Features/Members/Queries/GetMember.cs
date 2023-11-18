using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Extensions;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Dto;
using Auctioneer.Application.Features.Members.Errors;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.Queries;

public class GetMemberController : ApiControllerBase
{
    private readonly ILogger<GetMemberController> _logger;

    public GetMemberController(ILogger<GetMemberController> logger) : base(logger)
    {
        _logger = logger;
    }

    [HttpGet("api/member/{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MemberDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get([FromRoute] Guid id)
    {
        try
        {
            var query = new GetMemberQuery { Id = id };

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

public class GetMemberQuery : IRequest<Result<MemberDto>>
{
    public Guid Id { get; init; }
}

public class GetMemberQueryHandler : IRequestHandler<GetMemberQuery, Result<MemberDto>>
{
    private readonly IRepository<Member> _repository;

    public GetMemberQueryHandler(IRepository<Member> repository)
    {
        _repository = repository;
    }

    public async Task<Result<MemberDto>> Handle(GetMemberQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var member = await _repository.GetAsync(request.Id);

            if (member is null)
                return Result.Fail(new MemberNotFoundError());

            return Result.Ok(member.ToDto());
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}