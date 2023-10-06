using Auctioneer.Application.Common.Extensions;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Dto;
using HotChocolate.Execution;

namespace Auctioneer.GraphQL.Members;

[ExtendObjectType("Query")]
public class MemberQueries
{
    public async Task<List<MemberDto>> GetMembers([Service] IRepository<Member> memberRepository)
    {
        var result = await memberRepository.GetAsync();

        if (result is null)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage("No member found.")
                    .SetCode("NOT_FOUND")
                    .Build());
        }

        return result.ToDtos();
    }

    public async Task<MemberDto> GetMember([Service] IRepository<Member> memberRepository, Guid memberId)
    {
        var result = await memberRepository.GetAsync(memberId);

        if (result is null)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage("No member found.")
                    .SetCode("NOT_FOUND")
                    .Build());
        }

        return result.ToDto();
    }
}