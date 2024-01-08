using System.Net;
using Auctioneer.API.IntegrationTests.Extensions;
using Auctioneer.Application.Features.Auctions.Dto;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("BaseIntegrationTest")]
public class GetAuctionTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetAuctionEndPoint_Should_Fetch_Auction_If_Auction_Exist()
    {
        var auction = await SetupAuction();

        var response = await Client.GetAsync($"https://localhost:7298/api/auction/{auction.Id}")
            .DeserializeResponseAsync<AuctionDto>();

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Value);
        Assert.IsType<AuctionDto>(response.Value);
    }

    [Fact]
    public async Task GetAuctionEndPoint_Should_Return_Not_Found_If_Auction_Does_Not_Exist()
    {
        var response = await Client.GetAsync($"https://localhost:7298/api/auction/{Guid.NewGuid()}")
            .DeserializeResponseAsync<string>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("No auction found", response.Value);
    }

    [Fact]
    public async Task GetAuctionEndPoint_Should_Return_Not_Found_When_Not_Passing_Guid_As_QueryParameter()
    {
        var response = await Client.GetAsync("https://localhost:7298/api/auction/id234");

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Application.Entities.Auction> SetupAuction()
    {
        var auction = Application.Entities.Auction.Create(
            Guid.NewGuid(),
            "TestAuction",
            "TestDescription",
            DateTimeOffset.Now.AddHours(6),
            DateTimeOffset.Now.AddDays(7),
            100,
            "../images.test.jpg").Value;

        await AuctionRepository.CreateAsync(auction, new CancellationToken());
        await UnitOfWork.SaveAsync();

        return auction;
    }
}