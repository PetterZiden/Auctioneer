using System.Net;
using Auctioneer.API.IntegrationTests.Extensions;
using Auctioneer.Application.Features.Auctions.Dto;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("BaseIntegrationTest")]
public class GetAuctionsTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetAuctionsEndPoint_Should_Fetch_Auctions_If_Auctions_Exist()
    {
        await SetupAuction();

        var response = await Client.GetAsync("https://localhost:7298/api/auctions")
            .DeserializeResponseAsync<List<AuctionDto>>();

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Value);
        Assert.Equal(2, response.Value.Count);
        Assert.IsType<AuctionDto>(response.Value.FirstOrDefault());
    }

    [Fact]
    public async Task GetAuctionsEndPoint_Should_Return_Not_Found_If_Auctions_Does_Not_Exist()
    {
        var response = await Client.GetAsync("https://localhost:7298/api/auctions").DeserializeResponseAsync<string>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("No auction found", response.Value);
    }

    private async Task SetupAuction()
    {
        var auction = Application.Entities.Auction.Create(
            Guid.NewGuid(),
            "TestAuction",
            "TestDescription",
            DateTimeOffset.Now.AddHours(6),
            DateTimeOffset.Now.AddDays(7),
            100,
            "../images.test.jpg").Value;

        var auction2 = Application.Entities.Auction.Create(
            Guid.NewGuid(),
            "TestAuction2",
            "TestDescription2",
            DateTimeOffset.Now.AddHours(6),
            DateTimeOffset.Now.AddDays(7),
            150,
            "../images.test2.jpg").Value;

        await AuctionRepository.CreateAsync(auction, new CancellationToken());
        await AuctionRepository.CreateAsync(auction2, new CancellationToken());
        await UnitOfWork.SaveAsync();
    }
}