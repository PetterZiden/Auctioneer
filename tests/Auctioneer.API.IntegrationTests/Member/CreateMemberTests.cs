using System.Net;
using System.Text;
using System.Text.Json;
using Auctioneer.API.IntegrationTests.Extensions;
using Auctioneer.Application.Features.Members.Contracts;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("BaseIntegrationTest")]
public class CreateMemberTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateMembersEndPoint_Should_Create_Members_And_Return_MemberId_If_Request_Is_Valid()
    {
        var request = new CreateMemberRequest("Test", "Testsson", "Testgatan 2", "12345", "Testholm", "Test@test.se",
            "0734443322");

        var response = await Client.PostAsync("https://localhost:7298/api/member",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<Guid>();

        Assert.True(response.IsSuccess);
        Assert.IsType<Guid>(response.Value);
    }

    [Fact]
    public async Task CreateMembersEndPoint_Should_Return_Bad_Request_If_Request_Is_Missing_FirstName()
    {
        var request = new CreateMemberRequest("", "Testsson", "Testgatan 2", "12345", "Testholm", "Test@test.se",
            "0734443322");

        var response = await Client.PostAsync("https://localhost:7298/api/member",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal("'First Name' must not be empty.", response.Value.FirstOrDefault());
    }

    [Fact]
    public async Task CreateMembersEndPoint_Should_Return_Bad_Request_If_Request_Is_Missing_LastName()
    {
        var request =
            new CreateMemberRequest("Test", "", "Testgatan 2", "12345", "Testholm", "Test@test.se", "0734443322");

        var response = await Client.PostAsync("https://localhost:7298/api/member",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal("'Last Name' must not be empty.", response.Value.FirstOrDefault());
    }

    [Fact]
    public async Task CreateMembersEndPoint_Should_Return_Bad_Request_If_Email_Is_Invalid_Email_Format()
    {
        var request = new CreateMemberRequest("Test", "Testsson", "Testgatan 2", "12345", "Testholm", "test.test.se",
            "0734443322");

        var response = await Client.PostAsync("https://localhost:7298/api/member",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(response.Value);
        Assert.Equal("'Email' is not a valid email address.", response.Value.FirstOrDefault());
    }
}