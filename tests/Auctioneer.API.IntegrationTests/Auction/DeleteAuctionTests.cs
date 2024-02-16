namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("Auctioneer Test Collection")]
[TestCaseOrderer("Auctioneer.API.IntegrationTests.Helpers.PriorityOrderer", "Auctioneer.API.IntegrationTests")]
public class DeleteAuctionTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact, TestPriority(0)]
    public async Task DeleteAuctionEndPoint_Should_Delete_Auctions_If_Auction_Exist()
    {
        var auctionId = await SetupAuction();

        var response = await Client.DeleteAsync($"https://localhost:7298/api/auction/{auctionId}");

        var auction = await AuctionRepository.GetAsync(auctionId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.IsSuccessStatusCode.Should().BeTrue();
        auction.Should().BeNull();
    }

    [Fact, TestPriority(1)]
    public async Task DeleteAuctionEndPoint_Should_Not_Delete_Auctions_If_Auction_Not_Found()
    {
        var response = await Client.DeleteAsync($"https://localhost:7298/api/auction/{Guid.NewGuid()}")
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().Be("No auction found");
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