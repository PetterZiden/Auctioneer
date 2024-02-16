using Auctioneer.Application.Common.Models;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("Auctioneer Test Collection")]
[TestCaseOrderer("Auctioneer.API.IntegrationTests.Helpers.PriorityOrderer", "Auctioneer.API.IntegrationTests")]
public class PlaceBidTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact, TestPriority(6)]
    public async Task PlaceBidEndPoint_Should_Place_Bid_On_Auction_If_Request_Is_Valid()
    {
        var memberId = await SeedMember();
        var auctionId = await SeedAuction(memberId);
        var request = new Bid
        {
            AuctionId = auctionId,
            MemberId = memberId,
            BidPrice = 150,
            TimeStamp = null
        };

        var response = await Client.PostAsync("https://localhost:7298/api/auction/place-bid",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var auction = await AuctionRepository.GetAsync(auctionId);
        var member = await MemberRepository.GetAsync(memberId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.IsSuccessStatusCode.Should().BeTrue();
        auction?.Bids.Should().HaveCount(1);
        member?.Bids.Should().HaveCount(1);
    }

    [Fact, TestPriority(0)]
    public async Task PlaceBidEndPoint_Should__Not_Place_Bid_On_Auction_If_Auction_Is_Not_Found()
    {
        var request = new Bid
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            BidPrice = 150,
            TimeStamp = null
        };

        var response = await Client.PostAsync("https://localhost:7298/api/auction/place-bid",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().Be("No auction found");
    }

    [Fact, TestPriority(1)]
    public async Task PlaceBidEndPoint_Should__Not_Place_Bid_On_Auction_If_Member_Is_Not_Found()
    {
        var auctionId = await SeedAuction();
        var request = new Bid
        {
            AuctionId = auctionId,
            MemberId = Guid.NewGuid(),
            BidPrice = 150,
            TimeStamp = null
        };

        var response = await Client.PostAsync("https://localhost:7298/api/auction/place-bid",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().Be("No member found");
    }

    [Fact, TestPriority(2)]
    public async Task PlaceBidEndPoint_Should__Not_Place_Bid_On_Auction_If_BidPrice_Is_Less_Or_Equal_To_CurrentPrice()
    {
        var memberId = await SeedMember();
        var auctionId = await SeedAuction(memberId);
        var request = new Bid
        {
            AuctionId = auctionId,
            MemberId = memberId,
            BidPrice = 100,
            TimeStamp = null
        };

        var response = await Client.PostAsync("https://localhost:7298/api/auction/place-bid",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value.Should().StartWith("Bid must be greater than current price:");
    }

    [Fact, TestPriority(3)]
    public async Task PlaceBidEndPoint_Should__Not_Place_Bid_On_Auction_If_AuctionId_Is_Empty()
    {
        var request = new Bid
        {
            AuctionId = Guid.Empty,
            MemberId = Guid.NewGuid(),
            BidPrice = 150,
            TimeStamp = null
        };

        var response = await Client.PostAsync("https://localhost:7298/api/auction/place-bid",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Auction Id' must not be empty.");
    }

    [Fact, TestPriority(4)]
    public async Task PlaceBidEndPoint_Should__Not_Place_Bid_On_Auction_If_MemberId_Is_Empty()
    {
        var request = new Bid
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.Empty,
            BidPrice = 150,
            TimeStamp = null
        };

        var response = await Client.PostAsync("https://localhost:7298/api/auction/place-bid",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Member Id' must not be empty.");
    }

    [Fact, TestPriority(5)]
    public async Task PlaceBidEndPoint_Should__Not_Place_Bid_On_Auction_If_BidPrice_Is_Less_Than_0()
    {
        var request = new Bid
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            BidPrice = 0,
            TimeStamp = null
        };

        var response = await Client.PostAsync("https://localhost:7298/api/auction/place-bid",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Bid Price' must be greater than '0'.");
    }
}