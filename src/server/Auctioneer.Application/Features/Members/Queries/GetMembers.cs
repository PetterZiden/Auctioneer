using System.Reflection;
using Auctioneer.Application.Auth.Services;
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

public class GetMembersController(ILogger<GetMembersController> logger, CurrentUserService currentUser)
    : ApiControllerBase(logger)
{
    private readonly CurrentUserService _currentUser = currentUser;

    [HttpGet("api/members")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<MemberDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get()
    {
        using var _ = AuctioneerMetrics.MeasureRequestDuration();
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
            logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            return StatusCode(500);
        }
        finally
        {
            AuctioneerMetrics.IncreaseAuctioneerRequestCount();
        }
    }
}

public class GetMembersQuery : IRequest<Result<List<MemberDto>>>
{
}

public class GetMembersQueryHandler(IRepository<Member> repository)
    : IRequestHandler<GetMembersQuery, Result<List<MemberDto>>>
{
    public async Task<Result<List<MemberDto>>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var members = await repository.GetAsync();

            if (members is null || members.Count == 0)
                return Result.Fail(new MemberNotFoundError());

            return Result.Ok(members.ToDtos());
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}