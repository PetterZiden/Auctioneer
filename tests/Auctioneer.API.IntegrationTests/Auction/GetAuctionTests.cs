using System.Net;
using System.Text.Json;
using Auctioneer.Application.Features.Auctions.Dto;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("BaseIntegrationTest")]
public class GetAuctionTests : BaseIntegrationTest
{
    private readonly AuctioneerApiFactory _factory;

    public GetAuctionTests(AuctioneerApiFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAuctionEndPoint_Should_Fetch_Auction_If_Auction_Exist()
    {
        var auction = await SetupAuction();
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"https://localhost:7298/api/auction/{auction.Id}");
        var auctions = JsonSerializer.Deserialize<AuctionDto>(await response.Content.ReadAsStringAsync());

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(auctions);
        Assert.IsType<AuctionDto>(auctions);
    }

    [Fact]
    public async Task GetAuctionEndPoint_Should_Return_Not_Found_If_Auction_Does_Not_Exist()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"https://localhost:7298/api/auction/{Guid.NewGuid()}");
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        Assert.Equal("No auction found", errorMsg);
    }

    [Fact]
    public async Task GetAuctionEndPoint_Should_Return_Not_Found_When_Not_Passing_Guid_As_QueryParameter()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"https://localhost:7298/api/auction/id234");

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
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
            "../images.test.jpg");

        await AuctionRepository.CreateAsync(auction, new CancellationToken());
        await UnitOfWork.SaveAsync();

        return auction;
    }
}