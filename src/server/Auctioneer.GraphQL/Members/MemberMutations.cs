using System.Reflection;
using Auctioneer.Application.Features.Members.Commands;
using Auctioneer.GraphQL.Common;
using Auctioneer.GraphQL.Members.Inputs;
using Auctioneer.GraphQL.Members.Payloads;
using HotChocolate.Execution;
using MediatR;

namespace Auctioneer.GraphQL.Members;

[ExtendObjectType("Mutation")]
public class MemberMutations
{
    private readonly IMediator _mediator;
    private readonly ILogger<MemberMutations> _logger;

    public MemberMutations(IMediator mediator, ILogger<MemberMutations> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<CreateMemberPayload> CreateMember(CreateMemberInput input, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateMemberCommand
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                Street = input.Street,
                ZipCode = input.ZipCode,
                City = input.City,
                Email = input.Email,
                PhoneNumber = input.PhoneNumber
            };

            var validationResult = await new CreateMemberCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(validationResult));
            }

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
            }

            return new CreateMemberPayload(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<UpdateMemberPayload> UpdateMember(UpdateMemberInput input, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateMemberCommand
            {
                Id = input.MemberId,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Street = input.Street,
                Zipcode = input.ZipCode,
                City = input.City,
                PhoneNumber = input.PhoneNumber
            };

            var validationResult = await new UpdateMemberCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(validationResult));
            }


            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
            }

            return new UpdateMemberPayload("Member updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<DeleteMemberPayload> DeleteMember(DeleteMemberInput input, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteMemberCommand
            {
                MemberId = input.MemberId
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
            }

            return new DeleteMemberPayload(input.MemberId, "Member deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<ChangeEmailPayload> ChangeEmail(ChangeEmailInput input, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ChangeEmailMemberCommand { MemberId = input.MemberId, Email = input.Email };

            var validationResult =
                await new ChangeEmailMemberCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(validationResult));
            }

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
            }

            return new ChangeEmailPayload($"Email for member with id: {input.MemberId} changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }

    public async Task<RateMemberPayload> RateMember(RateMemberInput input, CancellationToken cancellationToken)
    {
        try
        {
            var command = new RateMemberCommand
            {
                RatingForMemberId = input.RatingForMemberId,
                RatingFromMemberId = input.RatingFromMemberId,
                Stars = input.Stars
            };

            var validationResult = await new RateMemberCommandValidator().ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(validationResult));
            }

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.IsSuccess)
            {
                throw new QueryException(CustomErrorBuilder.CreateError(result.Errors));
            }

            return new RateMemberPayload("Member rated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new QueryException(
                ErrorBuilder.New()
                    .SetMessage(ex.Message)
                    .SetCode("INTERNAL_ERROR")
                    .Build());
        }
    }
}