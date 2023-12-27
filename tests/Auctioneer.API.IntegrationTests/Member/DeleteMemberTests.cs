using System.Net;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("BaseIntegrationTest")]
public class DeleteMemberTests : BaseIntegrationTest
{
    private readonly AuctioneerApiFactory _factory;

    public DeleteMemberTests(AuctioneerApiFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeleteMemberEndPoint_Should_Delete_Members_If_Member_Exist()
    {
        var memberId = await SetupMember();

        var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"https://localhost:7298/api/member/{memberId}");

        var member = await MemberRepository.GetAsync(memberId);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Null(member);
    }

    [Fact]
    public async Task DeleteMemberEndPoint_Should_Not_Delete_Members_If_Member_Not_Found()
    {
        var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"https://localhost:7298/api/member/{Guid.NewGuid()}");
        var errorMsg = await response.Content.ReadAsStringAsync();

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        Assert.NotNull(errorMsg);
        Assert.Equal("No member found", errorMsg);
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