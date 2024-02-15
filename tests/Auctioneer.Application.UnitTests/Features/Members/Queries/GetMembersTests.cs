using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Dto;
using Auctioneer.Application.Features.Members.Queries;

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

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().AllBeOfType<MemberDto>();
        await _memberRepository.Received(1).GetAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_No_Member_Found()
    {
        _memberRepository.GetAsync().Returns([]);

        var result = await _handler.Handle(new GetMembersQuery(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("No member found");
        await _memberRepository.Received(1).GetAsync();
    }

    [Fact]
    public async void Should_Return_FailedResult_When_MemberRepository_Throws_Exception()
    {
        _memberRepository.GetAsync()!.Returns(
            Task.FromException<List<Member>>(new Exception("MemberRepository failed")));

        var result = await _handler.Handle(new GetMembersQuery(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("MemberRepository failed");
        await _memberRepository.Received(1).GetAsync();
    }
}