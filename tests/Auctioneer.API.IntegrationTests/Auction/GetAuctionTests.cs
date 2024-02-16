using Auctioneer.Application.Features.Auctions.Dto;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("Auctioneer Test Collection")]
[TestCaseOrderer("Auctioneer.API.IntegrationTests.Helpers.PriorityOrderer", "Auctioneer.API.IntegrationTests")]
public class GetAuctionTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact, TestPriority(2)]
    public async Task GetAuctionEndPoint_Should_Fetch_Auction_If_Auction_Exist()
    {
        var auctionId = await SeedAuction();

        var response = await Client.GetAsync($"https://localhost:7298/api/auction/{auctionId}")
            .DeserializeResponseAsync<AuctionDto>();

        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Value.Should().NotBeNull();
        response.Value.Should().BeOfType<AuctionDto>();
    }

    [Fact, TestPriority(1)]
    public async Task GetAuctionEndPoint_Should_Return_Not_Found_If_Auction_Does_Not_Exist()
    {
        var response = await Client.GetAsync($"https://localhost:7298/api/auction/{Guid.NewGuid()}")
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().Be("No auction found");
    }

    [Fact, TestPriority(0)]
    public async Task GetAuctionEndPoint_Should_Return_Not_Found_When_Not_Passing_Guid_As_QueryParameter()
    {
        var response = await Client.GetAsync("https://localhost:7298/api/auction/id234");

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}