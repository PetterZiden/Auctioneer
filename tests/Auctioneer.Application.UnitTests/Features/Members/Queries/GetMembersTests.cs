using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Dto;
using Auctioneer.Application.Features.Members.Queries;
using NSubstitute;

namespace Auctioneer.Application.UnitTests.Features.Members.Queries;

public class GetMembersTests
{
    private readonly IRepository<Member> _memberRepository;
    private readonly GetMembersQueryHandler _handler;

    public GetMembersTests()
    {
        _memberRepository = Substitute.For<IRepository<Member>>();
        _handler = new GetMembersQueryHandler(_memberRepository);
    }

    [Fact]
    public async void Should_Return_SuccessResult_And_MemberDto()
    {
        var listOfValidMembers = new List<Member>
        {
            GetValid.Member(),
            GetValid.Member()
        };

        _memberRepository.GetAsync().Returns(listOfValidMembers);

        var result = await _handler.Handle(new GetMembersQuery(), new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.IsType<List<MemberDto>>(result.Value);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_No_Member_Found()
    {
        _memberRepository.GetAsync().Returns(new List<Member>());

        var result = await _handler.Handle(new GetMembersQuery(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("No member found", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_MemberRepository_Throws_Exception()
    {
        _memberRepository.GetAsync()!.Returns(
            Task.FromException<List<Member>>(new Exception("MemberRepository failed")));

        var result = await _handler.Handle(new GetMembersQuery(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("MemberRepository failed", result.Errors[0].Message);
    }
}