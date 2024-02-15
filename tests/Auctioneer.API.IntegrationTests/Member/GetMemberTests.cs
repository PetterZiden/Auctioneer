using Auctioneer.Application.Features.Members.Dto;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("BaseIntegrationTest")]
public class GetMemberTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetMemberEndPoint_Should_Fetch_Member_If_Member_Exist()
    {
        var member = await SetupMember();

        var response = await Client.GetAsync($"https://localhost:7298/api/member/{member.Id}")
            .DeserializeResponseAsync<MemberDto>();

        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Value.Should().NotBeNull();
        response.Value.Should().BeOfType<MemberDto>();
    }

    [Fact]
    public async Task GetMemberEndPoint_Should_Return_Not_Found_If_Member_Does_Not_Exist()
    {
        var response = await Client.GetAsync($"https://localhost:7298/api/member/{Guid.NewGuid()}")
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("No member found");
    }

    [Fact]
    public async Task GetMemberEndPoint_Should_Return_Not_Found_When_Not_Passing_Guid_As_QueryParameter()
    {
        var response = await Client.GetAsync("https://localhost:7298/api/member/id234");

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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