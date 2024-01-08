using System.Net;
using Auctioneer.API.IntegrationTests.Extensions;
using Auctioneer.Application.Features.Members.Dto;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("BaseIntegrationTest")]
public class GetMembersTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetMembersEndPoint_Should_Fetch_Members_If_Members_Exist()
    {
        await SetupMember();

        var response = await Client.GetAsync("https://localhost:7298/api/members")
            .DeserializeResponseAsync<List<MemberDto>>();

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Value);
        Assert.Equal(2, response.Value.Count);
        Assert.IsType<MemberDto>(response.Value.FirstOrDefault());
    }

    [Fact]
    public async Task GetMembersEndPoint_Should_Return_Not_Found_If_Members_Does_Not_Exist()
    {
        var response = await Client.GetAsync("https://localhost:7298/api/members").DeserializeResponseAsync<string>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("No member found", response.Value);
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
            "testholm").Value;

        var member2 = Application.Entities.Member.Create(
            "Test2",
            "Testsson2",
            "test2@test.se",
            "0732223344",
            "testgatan 22",
            "12342",
            "testhol2").Value;

        await MemberRepository.CreateAsync(member, new CancellationToken());
        await MemberRepository.CreateAsync(member2, new CancellationToken());
        await UnitOfWork.SaveAsync();
    }
}