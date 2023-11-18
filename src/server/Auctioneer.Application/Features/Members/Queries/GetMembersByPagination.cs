using System.Reflection;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Extensions;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Common.Validators;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Contracts;
using Auctioneer.Application.Features.Members.Dto;
using Auctioneer.Application.Features.Members.Errors;
using Auctioneer.Application.Infrastructure.Persistence;
using FluentResults;
using MassTransit.Futures.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Auctioneer.Application.Features.Members.Queries;

public class GetMembersByPaginationController : ApiControllerBase
{
    private readonly ILogger<GetMembersByPaginationController> _logger;

    public GetMembersByPaginationController(ILogger<GetMembersByPaginationController> logger) : base(logger)
    {
        _logger = logger;
    }

    [HttpGet("api/members/{pageNumber:int}/{pageSize:int}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(GetMembersByPaginationResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> Get([FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        try
        {
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var validationResult = await new PaginationParamsValidator().ValidateAsync(paginationParams);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.ConvertAll(x => x.ErrorMessage);
                return BadRequest(errorMessages);
            }

            var query = new GetMembersByPaginationQuery
            {
                PaginationParams = paginationParams
            };

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

public class GetMembersByPaginationQuery : IRequest<Result<GetMembersByPaginationResponse>>
{
    public PaginationParams PaginationParams { get; init; }
}

public class
    GetMembersByPaginationQueryHandler : IRequestHandler<GetMembersByPaginationQuery,
        Result<GetMembersByPaginationResponse>>
{
    private readonly IRepository<Member> _repository;

    public GetMembersByPaginationQueryHandler(IRepository<Member> repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetMembersByPaginationResponse>> Handle(GetMembersByPaginationQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var (totalPages, members) =
                await _repository.GetAsync(request.PaginationParams.PageNumber, request.PaginationParams.PageSize);

            if (!members?.Any() == true)
                return Result.Fail(new MemberNotFoundError());

            var response = new GetMembersByPaginationResponse
            {
                TotalPages = totalPages,
                PageNumber = request.PaginationParams.PageNumber,
                Members = members.ToDtos()
            };
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message));
        }
    }
}