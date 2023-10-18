using System.Net;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("BaseIntegrationTest")]
public class DeleteAuctionTests : BaseIntegrationTest
{
    private readonly AuctioneerApiFactory _factory;

    public DeleteAuctionTests(AuctioneerApiFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeleteAuctionEndPoint_Should_Delete_Auctions_If_Auction_Exist()
    {
        var auctionId = await SetupAuction();

        var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"https://localhost:7298/api/auction/{auctionId}");

        var auction = await AuctionRepository.GetAsync(auctionId);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Null(auction);
    }

    [Fact]
    public async Task DeleteAuctionEndPoint_Should_Not_Delete_Auctions_If_Auction_Not_Found()
    {
        var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"https://localhost:7298/api/auction/{Guid.NewGuid()}");
        var errorMsg = await response.Content.ReadAsStringAsync();

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        Assert.NotNull(errorMsg);
        Assert.Equal("No auction found", errorMsg);
    }

    private async Task<Guid> SetupAuction()
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

        return auction.Id;
    }
}