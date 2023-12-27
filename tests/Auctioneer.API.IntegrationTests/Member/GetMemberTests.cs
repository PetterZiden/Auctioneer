using System.Net;
using System.Text.Json;
using Auctioneer.Application.Features.Members.Dto;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("BaseIntegrationTest")]
public class GetMemberTests : BaseIntegrationTest
{
    private readonly AuctioneerApiFactory _factory;

    public GetMemberTests(AuctioneerApiFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMemberEndPoint_Should_Fetch_Member_If_Member_Exist()
    {
        var member = await SetupMember();
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"https://localhost:7298/api/member/{member.Id}");
        var members = JsonSerializer.Deserialize<MemberDto>(await response.Content.ReadAsStringAsync());

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(members);
        Assert.IsType<MemberDto>(members);
    }

    [Fact]
    public async Task GetMemberEndPoint_Should_Return_Not_Found_If_Member_Does_Not_Exist()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"https://localhost:7298/api/member/{Guid.NewGuid()}");
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        Assert.Equal("No member found", errorMsg);
    }

    [Fact]
    public async Task GetMemberEndPoint_Should_Return_Not_Found_When_Not_Passing_Guid_As_QueryParameter()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("https://localhost:7298/api/member/id234");

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
    }

    private async Task<Application.Entities.Member> SetupMember()
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

        return member;
    }
}