using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Dto;
using Auctioneer.Application.Features.Members.Queries;
using NSubstitute;

namespace Auctioneer.Application.UnitTests.Features.Members.Queries;

public class GetMemberTests
{
    private readonly IRepository<Member> _memberRepository;
    private readonly GetMemberQueryHandler _handler;

    public GetMemberTests()
    {
        _memberRepository = Substitute.For<IRepository<Member>>();
        _handler = new GetMemberQueryHandler(_memberRepository);
    }

    [Fact]
    public async void Should_Return_SuccessResult_And_MemberDto()
    {
        var validMember = GetValid.Member();
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns(validMember);

        var result = await _handler.Handle(GetValidQuery(), new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.IsType<MemberDto>(result.Value);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_No_Member_Found()
    {
        _memberRepository.GetAsync(Arg.Any<Guid>()).Returns((Member?)null);

        var result = await _handler.Handle(GetValidQuery(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("No member found", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_MemberRepository_Throws_Exception()
    {
        _memberRepository.GetAsync(Arg.Any<Guid>())!.Returns(
            Task.FromException<Member>(new Exception("MemberRepository failed")));

        var result = await _handler.Handle(GetValidQuery(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("MemberRepository failed", result.Errors[0].Message);
    }

    private static GetMemberQuery GetValidQuery()
    {
        return new GetMemberQuery
        {
            Id = Guid.NewGuid()
        };
    }
}