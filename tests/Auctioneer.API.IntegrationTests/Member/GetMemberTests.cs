using Auctioneer.Application.Features.Members.Dto;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("Auctioneer Test Collection")]
[TestCaseOrderer("Auctioneer.API.IntegrationTests.Helpers.PriorityOrderer", "Auctioneer.API.IntegrationTests")]
public class GetMemberTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact, TestPriority(2)]
    public async Task GetMemberEndPoint_Should_Fetch_Member_If_Member_Exist()
    {
        var memberId = await SeedMember();

        var response = await Client.GetAsync($"https://localhost:7298/api/member/{memberId}")
            .DeserializeResponseAsync<MemberDto>();

        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Value.Should().NotBeNull();
        response.Value.Should().BeOfType<MemberDto>();
    }

    [Fact, TestPriority(0)]
    public async Task GetMemberEndPoint_Should_Return_Not_Found_If_Member_Does_Not_Exist()
    {
        var response = await Client.GetAsync($"https://localhost:7298/api/member/{Guid.NewGuid()}")
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("No member found");
    }

    [Fact, TestPriority(1)]
    public async Task GetMemberEndPoint_Should_Return_Not_Found_When_Not_Passing_Guid_As_QueryParameter()
    {
        var response = await Client.GetAsync("https://localhost:7298/api/member/id234");

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}