using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Auctioneer.Application.Features.Members.Contracts;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("BaseIntegrationTest")]
public class UpdateMemberTests : BaseIntegrationTest
{
    private readonly AuctioneerApiFactory _factory;

    public UpdateMemberTests(AuctioneerApiFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateMembersEndPoint_Should_Update_Members_If_Request_Is_Valid()
    {
        var memberId = await SetupMember();

        var request = new UpdateMemberRequest(memberId, "Test2", "Testsson2", "Testgatan 22", "54321", "Testborg",
            "0735554422");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/member",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var member = await MemberRepository.GetAsync(memberId);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(request.FirstName, member?.FirstName);
        Assert.Equal(request.LastName, member?.LastName);
        Assert.Equal(request.Street, member?.Address.Street);
        Assert.Equal(request.ZipCode, member?.Address.ZipCode);
        Assert.Equal(request.City, member?.Address.City);
        Assert.Equal(request.PhoneNumber, member?.PhoneNumber);
    }

    [Fact]
    public async Task UpdateMembersEndPoint_Should_Update_Members_Address()
    {
        var memberId = await SetupMember();

        var request = new UpdateMemberRequest(memberId, null, null, "Testgatan 22", "54321", "Testborg", null);
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/member",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var member = await MemberRepository.GetAsync(memberId);

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal(request.Street, member?.Address.Street);
        Assert.Equal(request.ZipCode, member?.Address.ZipCode);
        Assert.Equal(request.City, member?.Address.City);
    }

    [Fact]
    public async Task UpdateMembersEndPoint_Should_Update_Members_If_Member_Is_Not_Found()
    {
        var request = new UpdateMemberRequest(Guid.NewGuid(), "Test2", "Testsson2", "Testgatan 22", "54321", "Testborg",
            "0735554422");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/member",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound);
        Assert.NotNull(errorMsg);
        Assert.Equal("No member found", errorMsg);
    }

    [Fact]
    public async Task UpdateMembersEndPoint_Should_Not_Update_Members_If_PhoneNumber_Is_Same_As_Before()
    {
        var memberId = await SetupMember();

        var request = new UpdateMemberRequest(memberId, "Test2", "Testsson2", "Testgatan 22", "54321", "Testborg",
            "0734443322");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/member",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        var member = await MemberRepository.GetAsync(memberId);

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("Phone number can not be the same as current phone number", errorMsg);
        Assert.Equal("0734443322", member?.PhoneNumber);
    }

    [Fact]
    public async Task UpdateMembersEndPoint_Should_Not_Update_Members_If_LastName_Is_Same_As_Before()
    {
        var memberId = await SetupMember();

        var request = new UpdateMemberRequest(memberId, "Test2", "Testsson", "Testgatan 22", "54321", "Testborg",
            "0734443377");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/member",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        var member = await MemberRepository.GetAsync(memberId);

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("Last name can not be the same as current last name", errorMsg);
        Assert.Equal("Testsson", member?.LastName);
    }

    [Fact]
    public async Task UpdateMembersEndPoint_Should_Not_Update_Members_If_FirstName_Is_Same_As_Before()
    {
        var memberId = await SetupMember();

        var request = new UpdateMemberRequest(memberId, "Test", "Testsson2", "Testgatan 22", "54321", "Testborg",
            "0734443377");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PutAsync("https://localhost:7298/api/member",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));
        var errorMsg = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());

        var member = await MemberRepository.GetAsync(memberId);

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("First name can not be the same as current first name", errorMsg);
        Assert.Equal("Test", member?.FirstName);
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