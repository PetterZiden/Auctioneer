namespace Auctioneer.API.IntegrationTests.Member;

[Collection("Auctioneer Test Collection")]
[TestCaseOrderer("Auctioneer.API.IntegrationTests.Helpers.PriorityOrderer", "Auctioneer.API.IntegrationTests")]
public class DeleteMemberTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact, TestPriority(1)]
    public async Task DeleteMemberEndPoint_Should_Delete_Members_If_Member_Exist()
    {
        var memberId = await SeedMember();

        var response = await Client.DeleteAsync($"https://localhost:7298/api/member/{memberId}");

        var member = await MemberRepository.GetAsync(memberId);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        member.Should().BeNull();
    }

    [Fact, TestPriority(0)]
    public async Task DeleteMemberEndPoint_Should_Not_Delete_Members_If_Member_Not_Found()
    {
        var response = await Client.DeleteAsync($"https://localhost:7298/api/member/{Guid.NewGuid()}")
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("No member found");
    }
}