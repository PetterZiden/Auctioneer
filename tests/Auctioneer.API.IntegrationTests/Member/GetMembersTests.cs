using Auctioneer.Application.Features.Members.Dto;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("Auctioneer Test Collection")]
[TestCaseOrderer("Auctioneer.API.IntegrationTests.Helpers.PriorityOrderer", "Auctioneer.API.IntegrationTests")]
public class GetMembersTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact, TestPriority(1)]
    public async Task GetMembersEndPoint_Should_Fetch_Members_If_Members_Exist()
    {
        await SeedMembers();

        var response = await Client.GetAsync("https://localhost:7298/api/members")
            .DeserializeResponseAsync<List<MemberDto>>();

        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Value.Should().NotBeNull();
        response.Value.Should().HaveCount(2);
        response.Value.Should().AllBeOfType<MemberDto>();
    }

    [Fact, TestPriority(0)]
    public async Task GetMembersEndPoint_Should_Return_Not_Found_If_Members_Does_Not_Exist()
    {
        await ResetDb();
        var response = await Client.GetAsync("https://localhost:7298/api/members").DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("No member found");
    }
}