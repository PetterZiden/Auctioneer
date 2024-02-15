using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions.Contracts;
using Auctioneer.Application.Features.Auctions.Dto;
using Auctioneer.Application.Features.Auctions.Queries;
using Auctioneer.Application.Infrastructure.Persistence;

namespace Auctioneer.Application.UnitTests.Features.Auctions.Queries;

public class GetAuctionsByPaginationTests
{
    private readonly IRepository<Auction> _auctionRepository;
    private readonly GetAuctionsByPaginationQueryHandler _handler;

    public GetAuctionsByPaginationTests()
    {
        _auctionRepository = Substitute.For<IRepository<Auction>>();
        _handler = new GetAuctionsByPaginationQueryHandler(_auctionRepository);
    }

    [Fact]
    public async void Should_Return_SuccessResult_And_AuctionDto()
    {
        var listOfValidAuctions = new List<Auction>
        {
            GetValid.Auction(),
            GetValid.Auction()
        };

        _auctionRepository.GetAsync(Arg.Any<int>(), Arg.Any<int>()).Returns((1, listOfValidAuctions));

        var result = await _handler.Handle(new GetAuctionsByPaginationQuery
        {
            PaginationParams = new PaginationParams
            {
                PageNumber = 1,
                PageSize = 10
            }
        }, new CancellationToken());

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPages.Should().Be(1);
        result.Value.PageNumber.Should().Be(1);
        result.Value.Auctions.Should().AllBeOfType<AuctionDto>();
        result.Value.Should().BeOfType<GetAuctionsByPaginationResponse>();
        await _auctionRepository.Received(1).GetAsync(Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async void Should_Return_FailedResult_When_No_Auction_Found()
    {
        _auctionRepository.GetAsync(Arg.Any<int>(), Arg.Any<int>()).Returns((1, new List<Auction>()));

        var result = await _handler.Handle(new GetAuctionsByPaginationQuery
        {
            PaginationParams = new PaginationParams
            {
                PageNumber = 1,
                PageSize = 10
            }
        }, new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("No auction found");
        await _auctionRepository.Received(1).GetAsync(Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async void Should_Return_FailedResult_When_AuctionRepository_Throws_Exception()
    {
        _auctionRepository.GetAsync(Arg.Any<int>(), Arg.Any<int>())!.Returns(
            Task.FromException<(int, List<Auction>)>(new Exception("AuctionRepository failed")));

        var result = await _handler.Handle(new GetAuctionsByPaginationQuery
        {
            PaginationParams = new PaginationParams
            {
                PageNumber = 1,
                PageSize = 10
            }
        }, new CancellationToken());

        result.IsFailed.Should().BeTrue();
        result.Errors.FirstOrDefault()?.Message.Should().Be("AuctionRepository failed");
        await _auctionRepository.Received(1).GetAsync(Arg.Any<int>(), Arg.Any<int>());
    }
}