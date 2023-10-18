using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Dto;
using Auctioneer.Application.Features.Auctions.Queries;
using NSubstitute;

namespace Auctioneer.Application.UnitTests.Features.Auctions.Queries;

public class GetAuctionTests
{
    private readonly IRepository<Auction> _auctionRepository;
    private readonly GetAuctionQueryHandler _handler;

    public GetAuctionTests()
    {
        _auctionRepository = Substitute.For<IRepository<Auction>>();
        _handler = new GetAuctionQueryHandler(_auctionRepository);
    }

    [Fact]
    public async void Should_Return_SuccessResult_And_AuctionDto()
    {
        var validAuction = GetValid.Auction();
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns(validAuction);

        var result = await _handler.Handle(GetValidQuery(), new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.IsType<AuctionDto>(result.Value);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_No_Auction_Found()
    {
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns((Auction?)null);

        var result = await _handler.Handle(GetValidQuery(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("No auction found", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_AuctionRepository_Throws_Exception()
    {
        _auctionRepository.GetAsync(Arg.Any<Guid>())!.Returns(
            Task.FromException<Auction>(new Exception("AuctionRepository failed")));

        var result = await _handler.Handle(GetValidQuery(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("AuctionRepository failed", result.Errors[0].Message);
    }

    private static GetAuctionQuery GetValidQuery()
    {
        return new GetAuctionQuery
        {
            Id = Guid.NewGuid()
        };
    }
}