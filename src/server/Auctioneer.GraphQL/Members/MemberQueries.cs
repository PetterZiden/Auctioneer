using Auctioneer.Application.Features.Members.Dto;
using Auctioneer.Application.Features.Members.Queries;
using Auctioneer.GraphQL.Common;
using HotChocolate.Execution;
using MediatR;

namespace Auctioneer.GraphQL.Members;

[ExtendObjectType("Query")]
public class MemberQueries(ISender mediator, ILogger<MemberQueries> logger)
{
    public async Task<List<MemberDto>> GetMembers()
    {
        var query = new GetMembersQuery();

        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
        }

        return result.Value;
    }

    public async Task<MemberDto> GetMember(Guid memberId)
    {
        var query = new GetMemberQuery { Id = memberId };

        var result = await mediator.Send(query);

        if (!result.IsSuccess)
        {
            throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
        }

        return result.Value;
    }
}