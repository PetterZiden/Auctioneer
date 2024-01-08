using System.Net;
using System.Text;
using System.Text.Json;
using Auctioneer.API.IntegrationTests.Extensions;
using Auctioneer.Application.Features.Members.Contracts;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("BaseIntegrationTest")]
public class ChangeEmailMemberTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task ChangeMemberEmailEndPoint_Should_Update_Members_Email_If_Request_Is_Valid()
    {
        var memberId = await SetupMember();

        var request = new ChangeMemberEmailRequest(memberId, "newTest@test.se");

        var response = await Client.PutAsync("https://localhost:7298/api/member/change-email",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var member = await MemberRepository.GetAsync(memberId);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(request.Email, member?.Email.Value);
    }

    [Fact]
    public async Task ChangeMemberEmailEndPoint_Should_Not_Change_Members_Email_If_Member_Is_Not_Found()
    {
        var request = new ChangeMemberEmailRequest(Guid.NewGuid(), "newTest@test.se");

        var response = await Client.PutAsync("https://localhost:7298/api/member/change-email",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal("No member found", response.Value);
    }

    [Fact]
    public async Task ChangeMemberEmailEndPoint_Should_Not_Change_Members_Email_If_Email_Is_The_Same_As_Before()
    {
        var memberId = await SetupMember();

        var request = new ChangeMemberEmailRequest(memberId, "test@test.se");

        var response = await Client.PutAsync("https://localhost:7298/api/member/change-email",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal("Email can not be the same as current email", response.Value);
    }

    [Fact]
    public async Task ChangeMemberEndPoint_Should_Return_Bad_Request_If_Email_Is_Invalid_Email_Format()
    {
        var request = new ChangeMemberEmailRequest(Guid.NewGuid(), "newTest.test.se");

        var response = await Client.PutAsync("https://localhost:7298/api/member/change-email",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal("'Email' is not a valid email address.", response.Value.FirstOrDefault());
    }

    [Fact]
    public async Task ChangeMemberEndPoint_Should_Return_Bad_Request_If_MemberId_Is_Empty()
    {
        var request = new ChangeMemberEmailRequest(Guid.Empty, "newTest@test.se");

        var response = await Client.PutAsync("https://localhost:7298/api/member/change-email",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal("'Member Id' must not be empty.", response.Value.FirstOrDefault());
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