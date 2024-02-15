using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Dto;
using Auctioneer.Application.Features.Auctions.Queries;

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

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<AuctionDto>();
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async void Should_Return_FailedResult_When_No_Auction_Found()
    {
        _auctionRepository.GetAsync(Arg.Any<Guid>()).Returns((Auction?)null);

        var result = await _handler.Handle(GetValidQuery(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("No auction found");
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async void Should_Return_FailedResult_When_AuctionRepository_Throws_Exception()
    {
        _auctionRepository.GetAsync(Arg.Any<Guid>())!.Returns(
            Task.FromException<Auction>(new Exception("AuctionRepository failed")));

        var result = await _handler.Handle(GetValidQuery(), new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("AuctionRepository failed");
        await _auctionRepository.Received(1).GetAsync(Arg.Any<Guid>());
    }

    private static GetAuctionQuery GetValidQuery()
    {
        return new GetAuctionQuery
        {
            Id = Guid.NewGuid()
        };
    }
}