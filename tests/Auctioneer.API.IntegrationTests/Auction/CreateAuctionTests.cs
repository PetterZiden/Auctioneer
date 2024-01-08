using System.Net;
using System.Text;
using System.Text.Json;
using Auctioneer.API.IntegrationTests.Extensions;
using Auctioneer.Application.Features.Auctions.Contracts;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("BaseIntegrationTest")]
public class CreateAuctionTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
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

        var response = await Client.PostAsync("https://localhost:7298/api/auction",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<Guid>();

        Assert.True(response.IsSuccess);
        Assert.IsType<Guid>(response.Value);
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

        var response = await Client.PostAsync("https://localhost:7298/api/auction",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("No member found", response.Value);
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

        var response = await Client.PostAsync("https://localhost:7298/api/auction",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal("'Title' must not be empty.", response.Value.FirstOrDefault());
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

        var response = await Client.PostAsync("https://localhost:7298/api/auction",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal("'Description' must not be empty.", response.Value.FirstOrDefault());
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

        var response = await Client.PostAsync("https://localhost:7298/api/auction",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal("'Starting Price' must be greater than '-1'.", response.Value.FirstOrDefault());
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
            "testholm").Value;

        await MemberRepository.CreateAsync(member, new CancellationToken());
        await UnitOfWork.SaveAsync();

        return member.Id;
    }
}