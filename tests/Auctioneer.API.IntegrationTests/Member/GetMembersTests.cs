using System.Net;
using System.Text.Json;
using Auctioneer.Application.Features.Members.Dto;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("BaseIntegrationTest")]
public class GetMembersTests : BaseIntegrationTest
{
    private readonly AuctioneerApiFactory _factory;

    public GetMembersTests(AuctioneerApiFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMembersEndPoint_Should_Fetch_Members_If_Members_Exist()
    {
        await SetupMember();
        var client = _factory.CreateClient();

        var response = await client.GetAsync("https://localhost:7298/api/members");
        var content = await response.Content.ReadAsStringAsync();
        var members = JsonSerializer.Deserialize<List<MemberDto>>(content);

        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(members);
        Assert.Equal(2, members.Count);
        Assert.IsType<MemberDto>(members.FirstOrDefault());
    }

    [Fact]
    public async Task GetMembersEndPoint_Should_Return_Not_Found_If_Members_Does_Not_Exist()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("https://localhost:7298/api/members");
        var content = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        Assert.Equal("No member found", content);
    }

    private async Task SetupMember()
    {
        var member = Application.Entities.Member.Create(
            "Test",
            "Testsson",
            "test@test.se",
            "0734443322",
            "testgatan 2",
            "12345",
            "testholm");

        var member2 = Application.Entities.Member.Create(
            "Test2",
            "Testsson2",
            "test2@test.se",
            "0732223344",
            "testgatan 22",
            "12342",
            "testhol2");

        await MemberRepository.CreateAsync(member, new CancellationToken());
        await MemberRepository.CreateAsync(member2, new CancellationToken());
        await UnitOfWork.SaveAsync();
    }
}