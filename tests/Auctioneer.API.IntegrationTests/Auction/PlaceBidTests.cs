using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Auctioneer.Application.Common.Models;

namespace Auctioneer.API.IntegrationTests.Auction;

[Collection("BaseIntegrationTest")]
public class PlaceBidTests : BaseIntegrationTest
{
    private readonly AuctioneerApiFactory _factory;

    public PlaceBidTests(AuctioneerApiFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PlaceBidEndPoint_Should_Place_Bid_On_Auction_If_Request_Is_Valid()
    {
        var memberId = await SetupMember();
        var auctionId = await SetupAuction(memberId);
        var request = new Bid
        {
            AuctionId = auctionId,
            MemberId = memberId,
            BidPrice = 150,
            TimeStamp = null
        };

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/auction/place-bid",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var auction = await AuctionRepository.GetAsync(auctionId);
        var member = await MemberRepository.GetAsync(memberId);

        Assert.True(response.IsSuccessStatusCode);
        Assert.True(auction?.Bids.Count == 1);
        Assert.True(member?.Bids.Count == 1);
    }

    [Fact]
    public async Task PlaceBidEndPoint_Should__Not_Place_Bid_On_Auction_If_Auction_Is_Not_Found()
    {
        var request = new Bid
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            BidPrice = 150,
            TimeStamp = null
        };

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/auction/place-bid",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        Assert.NotNull(errorMsg);
        Assert.Equal("No auction found", errorMsg);
    }

    [Fact]
    public async Task PlaceBidEndPoint_Should__Not_Place_Bid_On_Auction_If_Member_Is_Not_Found()
    {
        var auctionId = await SetupAuction(Guid.NewGuid());
        var request = new Bid
        {
            AuctionId = auctionId,
            MemberId = Guid.NewGuid(),
            BidPrice = 150,
            TimeStamp = null
        };

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/auction/place-bid",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        Assert.NotNull(errorMsg);
        Assert.Equal("No member found", errorMsg);
    }

    [Fact]
    public async Task PlaceBidEndPoint_Should__Not_Place_Bid_On_Auction_If_BidPrice_Is_Less_Or_Equal_To_CurrentPrice()
    {
        var memberId = await SetupMember();
        var auctionId = await SetupAuction(memberId);
        var request = new Bid
        {
            AuctionId = auctionId,
            MemberId = memberId,
            BidPrice = 100,
            TimeStamp = null
        };

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/auction/place-bid",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.StartsWith("Bid must be greater than current price:", errorMsg);
    }

    [Fact]
    public async Task PlaceBidEndPoint_Should__Not_Place_Bid_On_Auction_If_AuctionId_Is_Empty()
    {
        var request = new Bid
        {
            AuctionId = Guid.Empty,
            MemberId = Guid.NewGuid(),
            BidPrice = 150,
            TimeStamp = null
        };

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/auction/place-bid",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("'Auction Id' must not be empty.", errorMsg.FirstOrDefault());
    }

    [Fact]
    public async Task PlaceBidEndPoint_Should__Not_Place_Bid_On_Auction_If_MemberId_Is_Empty()
    {
        var request = new Bid
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.Empty,
            BidPrice = 150,
            TimeStamp = null
        };

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/auction/place-bid",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("'Member Id' must not be empty.", errorMsg.FirstOrDefault());
    }

    [Fact]
    public async Task PlaceBidEndPoint_Should__Not_Place_Bid_On_Auction_If_BidPrice_Is_Less_Than_0()
    {
        var request = new Bid
        {
            AuctionId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            BidPrice = 0,
            TimeStamp = null
        };

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/auction/place-bid",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("'Bid Price' must be greater than '0'.", errorMsg.FirstOrDefault());
    }

    private async Task<Guid> SetupAuction(Guid memberId)
    {
        var auction = Application.Entities.Auction.Create(
            memberId,
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