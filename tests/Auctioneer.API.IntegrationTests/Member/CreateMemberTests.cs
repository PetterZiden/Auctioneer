using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Auctioneer.Application.Features.Members.Contracts;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("BaseIntegrationTest")]
public class CreateMemberTests : BaseIntegrationTest
{
    private readonly AuctioneerApiFactory _factory;

    public CreateMemberTests(AuctioneerApiFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateMembersEndPoint_Should_Create_Members_And_Return_MemberId_If_Request_Is_Valid()
    {
        var request = new CreateMemberRequest("Test", "Testsson", "Testgatan 2", "12345", "Testholm", "Test@test.se",
            "0734443322");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/member",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var memberId = JsonSerializer.Deserialize<Guid>(await response.Content.ReadAsStringAsync());

        Assert.True(response.IsSuccessStatusCode);
        Assert.IsType<Guid>(memberId);
    }

    [Fact]
    public async Task CreateMembersEndPoint_Should_Return_Bad_Request_If_Request_Is_Missing_FirstName()
    {
        var request = new CreateMemberRequest("", "Testsson", "Testgatan 2", "12345", "Testholm", "Test@test.se",
            "0734443322");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/member",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("'First Name' must not be empty.", errorMsg.FirstOrDefault());
    }

    [Fact]
    public async Task CreateMembersEndPoint_Should_Return_Bad_Request_If_Request_Is_Missing_LastName()
    {
        var request =
            new CreateMemberRequest("Test", "", "Testgatan 2", "12345", "Testholm", "Test@test.se", "0734443322");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/member",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("'Last Name' must not be empty.", errorMsg.FirstOrDefault());
    }

    [Fact]
    public async Task CreateMembersEndPoint_Should_Return_Bad_Request_If_Email_Is_Invalid_Email_Format()
    {
        var request = new CreateMemberRequest("Test", "Testsson", "Testgatan 2", "12345", "Testholm", "test.test.se",
            "0734443322");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync("https://localhost:7298/api/member",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var errorMsg = JsonSerializer.Deserialize<List<string>>(await response.Content.ReadAsStringAsync());

        Assert.False(response.IsSuccessStatusCode);
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        Assert.NotNull(errorMsg);
        Assert.Equal("'Email' is not a valid email address.", errorMsg.FirstOrDefault());
    }
}