using Auctioneer.Application.Common.Models;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("Auctioneer Test Collection")]
[TestCaseOrderer("Auctioneer.API.IntegrationTests.Helpers.PriorityOrderer", "Auctioneer.API.IntegrationTests")]
public class RateMemberTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact, TestPriority(1)]
    public async Task RateMemberEndPoint_Should_Add_Rating_To_Member_If_Request_Is_Valid()
    {
        var (memberId, member2Id) = await SeedMembers();

        var request = new Rating
        {
            RatingForMemberId = memberId,
            RatingFromMemberId = member2Id,
            Stars = 3
        };

        var response = await Client.PostAsync("https://localhost:7298/api/member/rate",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var member = await MemberRepository.GetAsync(memberId);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        member?.Ratings.Should().HaveCount(1);
        member?.NumberOfRatings.Should().Be(1);
    }

    [Fact, TestPriority(0)]
    public async Task RateMemberEndPoint_Should_Not_Add_Rating_To_Members_If_Member_Is_Not_Found()
    {
        var request = new Rating
        {
            RatingForMemberId = Guid.NewGuid(),
            RatingFromMemberId = Guid.NewGuid(),
            Stars = 3
        };

        var response = await Client.PostAsync("https://localhost:7298/api/member/rate",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("No member found");
    }

    [Fact, TestPriority(2)]
    public async Task RateMemberEndPoint_Should_Not_Add_Rating_To_Members_If_RatingFromMemberId_Is_Empty()
    {
        var request = new Rating
        {
            RatingForMemberId = Guid.NewGuid(),
            RatingFromMemberId = Guid.Empty,
            Stars = 3
        };

        var response = await Client.PostAsync("https://localhost:7298/api/member/rate",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Rating From Member Id' must not be empty.");
    }

    [Fact, TestPriority(3)]
    public async Task RateMemberEndPoint_Should_Not_Add_Rating_To_Members_If_RatingForMemberId_Is_Empty()
    {
        var request = new Rating
        {
            RatingForMemberId = Guid.Empty,
            RatingFromMemberId = Guid.NewGuid(),
            Stars = 3
        };

        var response = await Client.PostAsync("https://localhost:7298/api/member/rate",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Rating For Member Id' must not be empty.");
    }

    [Fact, TestPriority(4)]
    public async Task RateMemberEndPoint_Should_Not_Add_Rating_To_Members_If_Stars_Is_Less_Than_1()
    {
        var request = new Rating
        {
            RatingForMemberId = Guid.NewGuid(),
            RatingFromMemberId = Guid.NewGuid(),
            Stars = 0
        };

        var response = await Client.PostAsync("https://localhost:7298/api/member/rate",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Stars' must be greater than '0'.");
    }

    [Fact, TestPriority(5)]
    public async Task RateMemberEndPoint_Should_Not_Add_Rating_To_Members_If_Stars_Is_Greater_Than_5()
    {
        var request = new Rating
        {
            RatingForMemberId = Guid.NewGuid(),
            RatingFromMemberId = Guid.NewGuid(),
            Stars = 6
        };

        var response = await Client.PostAsync("https://localhost:7298/api/member/rate",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Stars' must be less than '6'.");
    }
}