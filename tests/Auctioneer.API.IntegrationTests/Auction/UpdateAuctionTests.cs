using Auctioneer.Application.Features.Auctions.Contracts;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("BaseIntegrationTest")]
public class UpdateAuctionTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task UpdateAuctionsEndPoint_Should_Update_Auctions_If_Request_Is_Valid()
    {
        var auctionId = await SetupAuction();

        var request = new UpdateAuctionRequest(auctionId, "TestAuction2", "TestDescription2", "../images/test2.jpg");

        var response = await Client.PutAsync("https://localhost:7298/api/auction",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var auction = await AuctionRepository.GetAsync(auctionId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.IsSuccessStatusCode.Should().BeTrue();
        auction?.Title.Should().Be(request.Title);
        auction?.Description.Should().Be(request.Description);
        auction?.ImgRoute.Should().Be(request.ImgRoute);
    }

    [Fact]
    public async Task UpdateAuctionsEndPoint_Should_Update_Auctions_If_Auction_Is_Not_Found()
    {
        var request =
            new UpdateAuctionRequest(Guid.NewGuid(), "TestAuction2", "TestDescription2", "../images/test2.jpg");

        var response = await Client.PutAsync("https://localhost:7298/api/auction",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().Be("No auction found");
    }

    [Fact]
    public async Task UpdateAuctionsEndPoint_Should_Not_Update_Auctions_If_Title_Is_Same_As_Before()
    {
        var auctionId = await SetupAuction();

        var request = new UpdateAuctionRequest(auctionId, "TestAuction", "TestDescription2", "../images/test2.jpg");

        var response = await Client.PutAsync("https://localhost:7298/api/auction",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        var auction = await AuctionRepository.GetAsync(auctionId);

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("Title can not be the same as current title");
        auction?.Title.Should().Be("TestAuction");
    }

    [Fact]
    public async Task UpdateAuctionsEndPoint_Should_Not_Update_Auctions_If_Description_Is_Same_As_Before()
    {
        var auctionId = await SetupAuction();

        var request = new UpdateAuctionRequest(auctionId, "TestAuction2", "TestDescription", "../images/test2.jpg");

        var response = await Client.PutAsync("https://localhost:7298/api/auction",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        var auction = await AuctionRepository.GetAsync(auctionId);

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("Description can not be the same as current description");
        auction?.Description.Should().Be("TestDescription");
    }

    [Fact]
    public async Task UpdateAuctionsEndPoint_Should_Not_Update_Auctions_If_ImageRoute_Is_Same_As_Before()
    {
        var auctionId = await SetupAuction();

        var request = new UpdateAuctionRequest(auctionId, "TestAuction2", "TestDescription2", "../images/test.jpg");

        var response = await Client.PutAsync("https://localhost:7298/api/auction",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        var auction = await AuctionRepository.GetAsync(auctionId);

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("Image route can not be the same as current image route");
        auction?.ImgRoute.Should().Be("../images/test.jpg");
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
            "../images/test.jpg").Value;

        await AuctionRepository.CreateAsync(auction, new CancellationToken());
        await UnitOfWork.SaveAsync();

        return auction.Id;
    }
}