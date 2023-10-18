using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.GraphQL.Members.Inputs;
using Auctioneer.GraphQL.Members.Payloads;
using HotChocolate.Execution;

namespace Auctioneer.GraphQL.Members;

[ExtendObjectType("Mutation")]
public class MemberMutations
{
    public async Task<CreateMemberPayload> CreateMember(CreateMemberInput input,
        [Service] IRepository<Member> memberRepository, CancellationToken cancellationToken)
    {
        try
        {
            var member = Member.Create(
                input.FirstName,
                input.LastName,
                input.Email,
                input.PhoneNumber,
                input.Street,
                input.ZipCode,
                input.City
            );

            await memberRepository.CreateAsync(member, cancellationToken);

            return new CreateMemberPayload(member.Id, member.Created);
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<UpdateMemberPayload> UpdateMember(UpdateMemberInput input,
        [Service] IRepository<Member> memberRepository, CancellationToken cancellationToken)
    {
        try
        {
            var member = await memberRepository.GetAsync(input.MemberId);

            if (member is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No member found.")
                        .SetCode("NOT_FOUND")
                        .Build());
            }

            await memberRepository.UpdateAsync(input.MemberId, member, cancellationToken);

            return new UpdateMemberPayload(member.Id, member.LastModified.Value);
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<DeleteMemberPayload> DeleteMember(DeleteMemberInput input,
        [Service] IRepository<Member> memberRepository, CancellationToken cancellationToken)
    {
        try
        {
            await memberRepository.DeleteAsync(input.MemberId, cancellationToken);

            return new DeleteMemberPayload(input.MemberId, "Member deleted successfully");
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<ChangeEmailPayload> ChangeEmail(ChangeEmailInput input,
        [Service] IRepository<Member> memberRepository, CancellationToken cancellationToken)
    {
        try
        {
            var member = await memberRepository.GetAsync(input.MemberId);

            if (member is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No member found.")
                        .SetCode("NOT_FOUND")
                        .Build());
            }

            var result = member.ChangeEmail(input.Email);

            if (!result.IsSuccess)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(result.Errors?.FirstOrDefault()?.Message)
                        .SetCode("BAD_REQUEST")
                        .Build());
            }

            await memberRepository.UpdateAsync(member.Id, member, cancellationToken);

            return new ChangeEmailPayload($"Email for member with id: {input.MemberId} changed successfully");
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<RateMemberPayload> RateMember(RateMemberInput input,
        [Service] IRepository<Member> memberRepository, CancellationToken cancellationToken)
    {
        try
        {
            var ratedMember = await memberRepository.GetAsync(input.RatingForMemberId);
            var ratedByMember = await memberRepository.GetAsync(input.RatingFromMemberId);

            if (ratedMember is null || ratedByMember is null)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage("No member found.")
                        .SetCode("NOT_FOUND")
                        .Build());
            }

            var result = ratedMember.Rate(input.RatingFromMemberId, input.Stars);

            if (!result.IsSuccess)
            {
                throw new QueryException(
                    ErrorBuilder.New()
                        .SetMessage(result.Errors?.FirstOrDefault()?.Message)
                        .SetCode("BAD_REQUEST")
                        .Build());
            }

            await memberRepository.UpdateAsync(ratedMember.Id, ratedMember, cancellationToken);

            return new RateMemberPayload("Member rated successfully");
        }
        catch (Exception ex)
        {
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }
}