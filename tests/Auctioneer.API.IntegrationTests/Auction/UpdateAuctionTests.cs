using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Auctioneer.Application.Features.Auctions.Contracts;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("BaseIntegrationTest")]
public class UpdateAuctionTests : BaseIntegrationTest
{
    private readonly AuctioneerApiFactory _factory;

    public UpdateAuctionTests(AuctioneerApiFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateAuctionsEndPoint_Should_Update_Auctions_If_Request_Is_Valid()
    {
        var auctionId = await SetupAuction();

        var request = new UpdateAuctionRequest(auctionId, "TestAuction2", "TestDescription2", "../images/test2.jpg");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/auction",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var auction = await AuctionRepository.GetAsync(auctionId);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(request.Title, auction?.Title);
        Assert.Equal(request.Description, auction?.Description);
        Assert.Equal(request.ImgRoute, auction?.ImgRoute);
    }

    [Fact]
    public async Task UpdateAuctionsEndPoint_Should_Update_Auctions_If_Auction_Is_Not_Found()
    {
        var request =
            new UpdateAuctionRequest(Guid.NewGuid(), "TestAuction2", "TestDescription2", "../images/test2.jpg");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/auction",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        Assert.NotNull(errorMsg);
        Assert.Equal("No auction found", errorMsg);
    }

    [Fact]
    public async Task UpdateAuctionsEndPoint_Should_Not_Update_Auctions_If_Title_Is_Same_As_Before()
    {
        var auctionId = await SetupAuction();

        var request = new UpdateAuctionRequest(auctionId, "TestAuction", "TestDescription2", "../images/test2.jpg");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/auction",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        var auction = await AuctionRepository.GetAsync(auctionId);

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("Title can not be the same as current title", errorMsg);
        Assert.Equal("TestAuction", auction?.Title);
    }

    [Fact]
    public async Task UpdateAuctionsEndPoint_Should_Not_Update_Auctions_If_Description_Is_Same_As_Before()
    {
        var auctionId = await SetupAuction();

        var request = new UpdateAuctionRequest(auctionId, "TestAuction2", "TestDescription", "../images/test2.jpg");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/auction",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        var auction = await AuctionRepository.GetAsync(auctionId);

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("Description can not be the same as current description", errorMsg);
        Assert.Equal("TestDescription", auction?.Description);
    }

    [Fact]
    public async Task UpdateAuctionsEndPoint_Should_Not_Update_Auctions_If_ImageRoute_Is_Same_As_Before()
    {
        var auctionId = await SetupAuction();

        var request = new UpdateAuctionRequest(auctionId, "TestAuction2", "TestDescription2", "../images/test.jpg");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/auction",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        var auction = await AuctionRepository.GetAsync(auctionId);

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("Image route can not be the same as current image route", errorMsg);
        Assert.Equal("../images/test.jpg", auction?.ImgRoute);
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
            "../images/test.jpg");

        await AuctionRepository.CreateAsync(auction, new CancellationToken());
        await UnitOfWork.SaveAsync();

        return auction.Id;
    }
}