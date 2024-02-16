using Auctioneer.Application.Features.Auctions.Contracts;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("Auctioneer Test Collection")]
[TestCaseOrderer("Auctioneer.API.IntegrationTests.Helpers.PriorityOrderer", "Auctioneer.API.IntegrationTests")]
public class CreateAuctionTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact, TestPriority(4)]
    public async Task CreateAuctionsEndPoint_Should_Create_Auctions_And_Return_AuctionId_If_Request_Is_Valid()
    {
        var memberId = await SeedMember();
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

        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Value.Should().NotBeEmpty();
    }

    [Fact, TestPriority(0)]
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

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().Be("No member found");
    }

    [Fact, TestPriority(1)]
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

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Title' must not be empty.");
    }

    [Fact, TestPriority(2)]
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

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Description' must not be empty.");
    }

    [Fact, TestPriority(3)]
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

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Starting Price' must be greater than '-1'.");
    }
}