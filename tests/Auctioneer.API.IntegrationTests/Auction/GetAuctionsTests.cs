using System.Net;
using System.Text.Json;
using Auctioneer.Application.Features.Auctions.Dto;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("BaseIntegrationTest")]
public class GetAuctionsTests : BaseIntegrationTest
{
    private readonly AuctioneerApiFactory _factory;

    public GetAuctionsTests(AuctioneerApiFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAuctionsEndPoint_Should_Fetch_Auctions_If_Auctions_Exist()
    {
        await SetupAuction();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("https://localhost:7298/api/auctions");
        var auctions = JsonSerializer.Deserialize<List<AuctionDto>>(await response.Content.ReadAsStringAsync());

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(auctions);
        Assert.Equal(2, auctions.Count);
        Assert.IsType<AuctionDto>(auctions.FirstOrDefault());
    }

    [Fact]
    public async Task GetAuctionsEndPoint_Should_Return_Not_Found_If_Auctions_Does_Not_Exist()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("https://localhost:7298/api/auctions");
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        Assert.Equal("No auction found", errorMsg);
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
            "../images.test.jpg");

        var auction2 = Application.Entities.Auction.Create(
            Guid.NewGuid(),
            "TestAuction2",
            "TestDescription2",
            DateTimeOffset.Now.AddHours(6),
            DateTimeOffset.Now.AddDays(7),
            150,
            "../images.test2.jpg");

        await AuctionRepository.CreateAsync(auction, new CancellationToken());
        await AuctionRepository.CreateAsync(auction2, new CancellationToken());
        await UnitOfWork.SaveAsync();
    }
}