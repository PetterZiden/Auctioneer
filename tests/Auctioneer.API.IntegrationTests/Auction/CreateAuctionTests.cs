using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Auctioneer.Application.Features.Auctions.Contracts;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("BaseIntegrationTest")]
public class CreateAuctionTests : BaseIntegrationTest
{
    private readonly AuctioneerApiFactory _factory;

    public CreateAuctionTests(AuctioneerApiFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateAuctionsEndPoint_Should_Create_Auctions_And_Return_AuctionId_If_Request_Is_Valid()
    {
        var memberId = await SetupMember();
        var request = new CreateAuctionRequest(
            memberId,
            "TestAuction",
            "TestDescription",
            DateTimeOffset.Now.AddHours(3),
            DateTimeOffset.Now.AddDays(7),
            100,
            "../images/test.jpg");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/auction",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var auctionId = JsonSerializer.Deserialize<Guid>(await response.Content.ReadAsStringAsync());

        Assert.True(response.IsSuccessStatusCode);
        Assert.IsType<Guid>(auctionId);
    }

    [Fact]
    public async Task CreateAuctionsEndPoint_Should_Not_Create_Auctions_If_Member_Is_Not_Found()
    {
        var request = new CreateAuctionRequest(
            Guid.NewGuid(),
            "TestAuction",
            "TestDescription",
            DateTimeOffset.Now.AddHours(3),
            DateTimeOffset.Now.AddDays(7),
            100,
            "../images/test.jpg");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/auction",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        Assert.Equal("No member found", errorMsg);
    }

    [Fact]
    public async Task CreateAuctionsEndPoint_Should_Return_Bad_Request_If_Request_Is_Missing_Title()
    {
        var request = new CreateAuctionRequest(
            Guid.NewGuid(),
            "",
            "TestDescription",
            DateTimeOffset.Now.AddHours(3),
            DateTimeOffset.Now.AddDays(7),
            100,
            "../images/test.jpg");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/auction",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("'Title' must not be empty.", errorMsg.FirstOrDefault());
    }

    [Fact]
    public async Task CreateAuctionsEndPoint_Should_Return_Bad_Request_If_Request_Is_Missing_Description()
    {
        var request = new CreateAuctionRequest(
            Guid.NewGuid(),
            "TestAuction",
            "",
            DateTimeOffset.Now.AddHours(3),
            DateTimeOffset.Now.AddDays(7),
            100,
            "../images/test.jpg");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/auction",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("'Description' must not be empty.", errorMsg.FirstOrDefault());
    }

    [Fact]
    public async Task CreateAuctionsEndPoint_Should_Return_Bad_Request_If_StartingPrice_Is_Less_Than_0()
    {
        var request = new CreateAuctionRequest(
            Guid.NewGuid(),
            "TestAuction",
            "TestDescription",
            DateTimeOffset.Now.AddHours(3),
            DateTimeOffset.Now.AddDays(7),
            -1,
            "../images/test.jpg");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/auction",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("'Starting Price' must be greater than '-1'.", errorMsg.FirstOrDefault());
    }

    private async Task<Guid> SetupMember()
    {
        var member = Application.Entities.Member.Create(
            "Test",
            "Testsson",
            "test@test.se",
            "0734443322",
            "testgatan 2",
            "12345",
            "testholm");

        await MemberRepository.CreateAsync(member, new CancellationToken());
        await UnitOfWork.SaveAsync();

        return member.Id;
    }
}