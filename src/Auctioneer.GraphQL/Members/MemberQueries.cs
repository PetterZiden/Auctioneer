using Auctioneer.Application.Features.Members.Dto;
using Auctioneer.Application.Features.Members.Queries;
using Auctioneer.GraphQL.Common;
using HotChocolate.Execution;
using MediatR;

namespace Auctioneer.GraphQL.Members;

[ExtendObjectType("Query")]
public class MemberQueries
{
    private readonly IMediator _mediator;
    private readonly ILogger<MemberQueries> _logger;

    public MemberQueries(IMediator mediator, ILogger<MemberQueries> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<List<MemberDto>> GetMembers()
    {
        var query = new GetMembersQuery();

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
        }

        return result.Value;
    }

    public async Task<MemberDto> GetMember(Guid memberId)
    {
        var query = new GetMemberQuery { Id = memberId };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
        }

        return result.Value;
    }
}