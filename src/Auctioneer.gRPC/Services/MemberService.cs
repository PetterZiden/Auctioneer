using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Helpers;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Features.Members.Commands;
using Auctioneer.Application.Features.Members.Dto;
using Auctioneer.gRPC.Mappers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Auctioneer.gRPC.Services;

public class MemberService : Member.MemberBase
{
    private readonly ILogger<MemberService> _logger;
    private readonly IRepository<Auctioneer.Application.Entities.Member> _memberRepository;
    private readonly IRepository<DomainEvent> _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MemberService(ILogger<MemberService> logger, IRepository<Application.Entities.Member> memberRepository,
        IRepository<DomainEvent> eventRepository, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _memberRepository = memberRepository;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public override async Task<MemberModel> GetMember(GetMemberRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.Id, out var memberId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));

            var member = await _memberRepository.GetAsync(memberId);

            if (member is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Member not found"));

            return Map.ApplicationMemberToMemberModel(member);
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task GetMembers(GetMembersRequest request, IServerStreamWriter<MemberModel> responseStream,
        ServerCallContext context)
    {
        try
        {
            var members = await _memberRepository.GetAsync();

            if (members is null || !members.Any())
                throw new RpcException(new Status(StatusCode.NotFound, "Member not found"));

            var memberModels = Map.ApplicationMemberToMemberModelList(members);

            foreach (var member in memberModels)
            {
                await responseStream.WriteAsync(member);
            }
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<CreateMemberResponse> CreateMember(MemberModel request, ServerCallContext context)
    {
        try
        {
            var cancellationToken = new CancellationToken();
            var member = Application.Entities.Member.Create(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.Street,
                request.Zipcode,
                request.City);

            var domainEvent = new MemberCreatedEvent(member, EventList.Member.MemberCreatedEvent);

            await _memberRepository.CreateAsync(member, cancellationToken);
            await _eventRepository.CreateAsync(domainEvent, cancellationToken);
            await _unitOfWork.SaveAsync();

            return new CreateMemberResponse
            {
                Id = member.Id.ToString(),
                CreatedAt = Timestamp.FromDateTimeOffset(member.Created)
            };
        }
        catch (Exception ex)
        {
            _unitOfWork.CleanOperations();
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<DeleteMemberResponse> DeleteMember(DeleteMemberRequest request,
        ServerCallContext context)
    {
        try
        {
            var cancellationToken = new CancellationToken();
            if (!Guid.TryParse(request.Id, out var memberId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));

            var domainEvent = new MemberDeletedEvent(memberId, EventList.Member.MemberDeletedEvent);

            await _memberRepository.DeleteAsync(memberId, cancellationToken);
            await _eventRepository.CreateAsync(domainEvent, cancellationToken);
            await _unitOfWork.SaveAsync();

            return new DeleteMemberResponse
            {
                Message = $"Member with id: {memberId} was successfully deleted"
            };
        }
        catch (Exception ex)
        {
            _unitOfWork.CleanOperations();
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<UpdateMemberResponse> UpdateMember(MemberModel request, ServerCallContext context)
    {
        try
        {
            var cancellationToken = new CancellationToken();
            if (!Guid.TryParse(request.Id, out var memberId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));

            var member = await _memberRepository.GetAsync(memberId);

            if (member is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Member not found"));

            var domainEvent = new MemberUpdatedEvent(member, EventList.Member.MemberUpdatedEvent);

            await _eventRepository.CreateAsync(domainEvent, cancellationToken);
            await _memberRepository.UpdateAsync(memberId, member, cancellationToken);
            await _unitOfWork.SaveAsync();

            return new UpdateMemberResponse
            {
                Id = request.Id,
                UpdatedAt = member.LastModified.HasValue
                    ? Timestamp.FromDateTimeOffset(member.LastModified.Value)
                    : null
            };
        }
        catch (Exception ex)
        {
            _unitOfWork.CleanOperations();
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<RateMemberResponse> RateMember(RateMemberRequest request, ServerCallContext context)
    {
        try
        {
            var cancellationToken = new CancellationToken();
            if (!Guid.TryParse(request.RatingForMemberId, out var memberForId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));

            if (!Guid.TryParse(request.RatingFromMemberId, out var memberFromId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MemberId was in wrong format"));

            var ratedMember = await _memberRepository.GetAsync(memberForId);
            var ratedByMember = await _memberRepository.GetAsync(memberFromId);

            if (ratedMember is null || ratedByMember is null)
                throw new RpcException(new Status(StatusCode.NotFound, "Member not found"));

            var result = ratedMember.Rate(memberFromId, request.Stars);

            if (!result.IsSuccess)
                throw new RpcException(new Status(StatusCode.InvalidArgument, result.Errors[0].Message));

            var rateMemberDto = new RateMemberDto
            {
                RatedName = ratedMember.FullName,
                RatedMemberId = ratedMember.Id,
                RatedEmail = ratedMember.Email,
                RatedByName = ratedByMember.FullName,
                Stars = request.Stars
            };
            var domainEvent = new RateMemberEvent(rateMemberDto, EventList.Member.RateMemberEvent);

            await _memberRepository.UpdateAsync(ratedMember.Id, ratedMember, cancellationToken);
            await _eventRepository.CreateAsync(domainEvent, cancellationToken);
            await _unitOfWork.SaveAsync();

            return new RateMemberResponse
            {
                Message = $"Member with id: {memberFromId} gave member with id: {memberForId} {request.Stars} stars"
            };
        }
        catch (Exception ex)
        {
            _unitOfWork.CleanOperations();
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}