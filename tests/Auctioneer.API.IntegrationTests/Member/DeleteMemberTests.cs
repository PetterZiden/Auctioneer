using System.Net;
using Auctioneer.API.IntegrationTests.Extensions;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("BaseIntegrationTest")]
public class DeleteMemberTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DeleteMemberEndPoint_Should_Delete_Members_If_Member_Exist()
    {
        var memberId = await SetupMember();

        var response = await Client.DeleteAsync($"https://localhost:7298/api/member/{memberId}");

        var member = await MemberRepository.GetAsync(memberId);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Null(member);
    }

    [Fact]
    public async Task DeleteMemberEndPoint_Should_Not_Delete_Members_If_Member_Not_Found()
    {
        var response = await Client.DeleteAsync($"https://localhost:7298/api/member/{Guid.NewGuid()}")
            .DeserializeResponseAsync<string>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal("No member found", response.Value);
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