using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Dto;
using Auctioneer.Application.Features.Auctions.Queries;
using NSubstitute;

namespace Auctioneer.Application.Tests.Unit.Features.Auctions.Queries;

public class GetAuctionsTests
{
    private readonly IRepository<Auction> _auctionRepository;
    private readonly GetAuctionsQueryHandler _handler;

    public GetAuctionsTests()
    {
        _auctionRepository = Substitute.For<IRepository<Auction>>();
        _handler = new GetAuctionsQueryHandler(_auctionRepository);
    }

    [Fact]
    public async void Should_Return_SuccessResult_And_AuctionDto()
    {
        var listOfValidAuctions = new List<Auction>
        {
            GetValid.Auction(),
            GetValid.Auction()
        };

        _auctionRepository.GetAsync().Returns(listOfValidAuctions);

        var result = await _handler.Handle(new GetAuctionsQuery(), new CancellationToken());

        Assert.True(result.IsSuccess);
        Assert.IsType<List<AuctionDto>>(result.Value);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_No_Auction_Found()
    {
        _auctionRepository.GetAsync().Returns(new List<Auction>());

        var result = await _handler.Handle(new GetAuctionsQuery(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("No auctions found", result.Errors[0].Message);
    }

    [Fact]
    public async void Should_Return_FailedResult_When_AuctionRepository_Throws_Exception()
    {
        _auctionRepository.GetAsync()!.Returns(
            Task.FromException<List<Auction>>(new Exception("AuctionRepository failed")));

        var result = await _handler.Handle(new GetAuctionsQuery(), new CancellationToken());

        Assert.True(result.IsFailed);
        Assert.Equal("AuctionRepository failed", result.Errors[0].Message);
    }
}