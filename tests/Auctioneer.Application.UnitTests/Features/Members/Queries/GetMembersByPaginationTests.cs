using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Members.Contracts;
using Auctioneer.Application.Features.Members.Dto;
using Auctioneer.Application.Features.Members.Queries;
using Auctioneer.Application.Infrastructure.Persistence;
using NSubstitute;

namespace Auctioneer.Application.UnitTests.Features.Members.Queries;

public class GetMembersByPaginationTests
{
    private readonly IRepository<Member> _memberRepository;
    private readonly GetMembersByPaginationQueryHandler _handler;

    public GetMembersByPaginationTests()
    {
        _memberRepository = Substitute.For<IRepository<Member>>();
        _handler = new GetMembersByPaginationQueryHandler(_memberRepository);
    }

    [Fact]
    public async void Should_Return_SuccessResult_And_AuctionDto()
    {
        var listOfValidMembers = new List<Member>
        {
            GetValid.Member(),
            GetValid.Member()
        };

        _memberRepository.GetAsync(Arg.Any<int>(), Arg.Any<int>()).Returns((1, listOfValidMembers));

        var result = await _handler.Handle(new GetMembersByPaginationQuery
        {
            PaginationParams = new PaginationParams
            {
                PageNumber = 1,
                PageSize = 10
            }
        }, new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalPages);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.IsType<List<MemberDto>>(result.Value.Members);
        Assert.IsType<GetMembersByPaginationResponse>(result.Value);
        await _memberRepository.Received(1).GetAsync(Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async void Should_Return_FailedResult_When_No_Auction_Found()
    {
        _memberRepository.GetAsync(Arg.Any<int>(), Arg.Any<int>()).Returns((1, new List<Member>()));

        var result = await _handler.Handle(new GetMembersByPaginationQuery
        {
            PaginationParams = new PaginationParams
            {
                PageNumber = 1,
                PageSize = 10
            }
        }, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("No member found", result.Errors[0].Message);
        await _memberRepository.Received(1).GetAsync(Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async void Should_Return_FailedResult_When_AuctionRepository_Throws_Exception()
    {
        _memberRepository.GetAsync(Arg.Any<int>(), Arg.Any<int>())!.Returns(
            Task.FromException<(int, List<Member>)>(new Exception("MemberRepository failed")));

        var result = await _handler.Handle(new GetMembersByPaginationQuery
        {
            PaginationParams = new PaginationParams
            {
                PageNumber = 1,
                PageSize = 10
            }
        }, new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("MemberRepository failed", result.Errors[0].Message);
        await _memberRepository.Received(1).GetAsync(Arg.Any<int>(), Arg.Any<int>());
    }
}