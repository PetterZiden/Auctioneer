using System.Reflection;
using Auctioneer.Application.Features.Members.Commands;
using Auctioneer.Application.Features.Members.Queries;
using Auctioneer.gRPC.Common;
using Auctioneer.gRPC.Mappers;
using Grpc.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Auctioneer.gRPC.Services;

[Authorize]
public class MemberService : Member.MemberBase
{
    private readonly ILogger<MemberService> _logger;
    private readonly IMediator _mediator;

    public MemberService(ILogger<MemberService> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public override async Task<MemberModel> GetMember(GetMemberRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var memberId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));

            var query = new GetMemberQuery { Id = memberId };

            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(result.Errors[0]));
            }

            return Map.MemberDtoToMemberModel(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task GetMembers(GetMembersRequest request, IServerStreamWriter<MemberModel> responseStream,
        ServerCallContext context)
    {
        try
        {
            var query = new GetMembersQuery();

            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(result.Errors[0]));
            }


            var memberModels = Map.MemberDtoToMemberModelList(result.Value);

            foreach (var member in memberModels)
            {
                await responseStream.WriteAsync(member);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<CreateMemberResponse> CreateMember(MemberModel request, ServerCallContext context)
    {
        try
        {
            var command = new CreateMemberCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Street = request.Street,
                ZipCode = request.Zipcode,
                City = request.City,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

            var validationResult = await new CreateMemberCommandValidator().ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(validationResult));
            }

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(result.Errors[0]));
            }

            return new CreateMemberResponse
            {
                Id = result.Value.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<DeleteMemberResponse> DeleteMember(DeleteMemberRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var memberId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));

            var command = new DeleteMemberCommand
            {
                MemberId = memberId
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(result.Errors[0]));
            }

            return new DeleteMemberResponse
            {
                Message = $"Member with id: {memberId} was successfully deleted"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<UpdateMemberResponse> UpdateMember(MemberModel request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var memberId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));

            var command = new UpdateMemberCommand
            {
                Id = memberId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Street = request.Street,
                Zipcode = request.Zipcode,
                City = request.City,
                PhoneNumber = request.PhoneNumber
            };

            var validationResult = await new UpdateMemberCommandValidator().ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(validationResult));
            }

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(result.Errors[0]));
            }

            return new UpdateMemberResponse
            {
                Message = "Member updated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<RateMemberResponse> RateMember(RateMemberRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.RatingForMemberId, out var memberForId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));

            if (!Guid.TryParse(request.RatingFromMemberId, out var memberFromId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));
            var command = new RateMemberCommand
            {
                RatingForMemberId = memberForId,
                RatingFromMemberId = memberFromId,
                Stars = request.Stars
            };

            var validationResult = await new RateMemberCommandValidator().ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(validationResult));
            }

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                throw new RpcException(CustomErrorBuilder.CreateError(result.Errors[0]));
            }

            return new RateMemberResponse
            {
                Message = $"Member with id: {memberFromId} gave member with id: {memberForId} {request.Stars} stars"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Name} threw exception", MethodBase.GetCurrentMethod()?.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}