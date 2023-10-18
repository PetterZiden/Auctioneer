using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Auctioneer.Application.Features.Members.Contracts;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("BaseIntegrationTest")]
public class ChangeEmailMemberTests : BaseIntegrationTest
{
    private readonly AuctioneerApiFactory _factory;

    public ChangeEmailMemberTests(AuctioneerApiFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ChangeMemberEmailEndPoint_Should_Update_Members_Email_If_Request_Is_Valid()
    {
        var memberId = await SetupMember();

        var request = new ChangeMemberEmailRequest(memberId, "newTest@test.se");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/member/change-email",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var member = await MemberRepository.GetAsync(memberId);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(request.Email, member?.Email);
    }

    [Fact]
    public async Task ChangeMemberEmailEndPoint_Should_Not_Change_Members_Email_If_Member_Is_Not_Found()
    {
        var request = new ChangeMemberEmailRequest(Guid.NewGuid(), "newTest@test.se");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/member/change-email",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        Assert.NotNull(errorMsg);
        Assert.Equal("No member found", errorMsg);
    }

    [Fact]
    public async Task ChangeMemberEmailEndPoint_Should_Not_Change_Members_Email_If_Email_Is_The_Same_As_Before()
    {
        var memberId = await SetupMember();

        var request = new ChangeMemberEmailRequest(memberId, "test@test.se");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/member/change-email",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("Email can not be the same as current email", errorMsg);
    }

    [Fact]
    public async Task ChangeMemberEndPoint_Should_Return_Bad_Request_If_Email_Is_Invalid_Email_Format()
    {
        var request = new ChangeMemberEmailRequest(Guid.NewGuid(), "newTest.test.se");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/member/change-email",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("'Email' is not a valid email address.", errorMsg.FirstOrDefault());
    }

    [Fact]
    public async Task ChangeMemberEndPoint_Should_Return_Bad_Request_If_MemberId_Is_Empty()
    {
        var request = new ChangeMemberEmailRequest(Guid.Empty, "newTest@test.se");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/member/change-email",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("'Member Id' must not be empty.", errorMsg.FirstOrDefault());
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
            "testholm");

        await MemberRepository.CreateAsync(member, new CancellationToken());
        await UnitOfWork.SaveAsync();

        return member.Id;
    }
}