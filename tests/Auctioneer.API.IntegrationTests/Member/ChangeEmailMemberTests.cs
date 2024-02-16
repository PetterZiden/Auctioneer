using Auctioneer.Application.Features.Members.Contracts;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("Auctioneer Test Collection")]
[TestCaseOrderer("Auctioneer.API.IntegrationTests.Helpers.PriorityOrderer", "Auctioneer.API.IntegrationTests")]
public class ChangeEmailMemberTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact, TestPriority(4)]
    public async Task ChangeMemberEmailEndPoint_Should_Update_Members_Email_If_Request_Is_Valid()
    {
        var memberId = await GetMemberId();

        var request = new ChangeMemberEmailRequest(memberId, "newTest@test.se");

        var response = await Client.PutAsync("https://localhost:7298/api/member/change-email",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var member = await MemberRepository.GetAsync(memberId);

        response.IsSuccessStatusCode.Should().BeTrue();
        member?.Email.Value.Should().Be(request.Email);
    }

    [Fact, TestPriority(0)]
    public async Task ChangeMemberEmailEndPoint_Should_Not_Change_Members_Email_If_Member_Is_Not_Found()
    {
        var request = new ChangeMemberEmailRequest(Guid.NewGuid(), "newTest@test.se");

        var response = await Client.PutAsync("https://localhost:7298/api/member/change-email",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("No member found");
    }

    [Fact, TestPriority(1)]
    public async Task ChangeMemberEmailEndPoint_Should_Not_Change_Members_Email_If_Email_Is_The_Same_As_Before()
    {
        var memberId = await SeedMember();

        var request = new ChangeMemberEmailRequest(memberId, "test@test.se");

        var response = await Client.PutAsync("https://localhost:7298/api/member/change-email",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("Email can not be the same as current email");
    }

    [Fact, TestPriority(2)]
    public async Task ChangeMemberEndPoint_Should_Return_Bad_Request_If_Email_Is_Invalid_Email_Format()
    {
        var request = new ChangeMemberEmailRequest(Guid.NewGuid(), "newTest.test.se");

        var response = await Client.PutAsync("https://localhost:7298/api/member/change-email",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Email' is not a valid email address.");
    }

    [Fact, TestPriority(3)]
    public async Task ChangeMemberEndPoint_Should_Return_Bad_Request_If_MemberId_Is_Empty()
    {
        var request = new ChangeMemberEmailRequest(Guid.Empty, "newTest@test.se");

        var response = await Client.PutAsync("https://localhost:7298/api/member/change-email",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Member Id' must not be empty.");
    }
}