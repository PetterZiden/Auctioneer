using System.Net;
using Auctioneer.API.IntegrationTests.Extensions;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("BaseIntegrationTest")]
public class DeleteAuctionTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DeleteAuctionEndPoint_Should_Delete_Auctions_If_Auction_Exist()
    {
        var auctionId = await SetupAuction();

        var response = await Client.DeleteAsync($"https://localhost:7298/api/auction/{auctionId}");

        var auction = await AuctionRepository.GetAsync(auctionId);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Null(auction);
    }

    [Fact]
    public async Task DeleteAuctionEndPoint_Should_Not_Delete_Auctions_If_Auction_Not_Found()
    {
        var response = await Client.DeleteAsync($"https://localhost:7298/api/auction/{Guid.NewGuid()}")
            .DeserializeResponseAsync<string>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal("No auction found", response.Value);
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
            "../images.test.jpg").Value;

        await AuctionRepository.CreateAsync(auction, new CancellationToken());
        await UnitOfWork.SaveAsync();

        return auction.Id;
    }
}