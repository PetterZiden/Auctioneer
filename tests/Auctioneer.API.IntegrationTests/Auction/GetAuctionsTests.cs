using Auctioneer.Application.Features.Auctions.Dto;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("Auctioneer Test Collection")]
[TestCaseOrderer("Auctioneer.API.IntegrationTests.Helpers.PriorityOrderer", "Auctioneer.API.IntegrationTests")]
public class GetAuctionsTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact, TestPriority(1)]
    public async Task GetAuctionsEndPoint_Should_Fetch_Auctions_If_Auctions_Exist()
    {
        await SeedAuctions();

        var response = await Client.GetAsync("https://localhost:7298/api/auctions")
            .DeserializeResponseAsync<List<AuctionDto>>();

        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Value.Should().NotBeNull();
        response.Value.Should().AllBeOfType<AuctionDto>();
        response.Value.Should().HaveCount(2);
    }

    [Fact, TestPriority(0)]
    public async Task GetAuctionsEndPoint_Should_Return_Not_Found_If_Auctions_Does_Not_Exist()
    {
        await ResetDb();
        var response = await Client.GetAsync("https://localhost:7298/api/auctions").DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().Be("No auction found");
    }
}