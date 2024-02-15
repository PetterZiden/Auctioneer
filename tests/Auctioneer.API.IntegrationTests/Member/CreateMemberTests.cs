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

        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateMembersEndPoint_Should_Return_Bad_Request_If_Request_Is_Missing_FirstName()
    {
        var request = new CreateMemberRequest("", "Testsson", "Testgatan 2", "12345", "Testholm", "Test@test.se",
            "0734443322");

        var response = await Client.PostAsync("https://localhost:7298/api/member",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'First Name' must not be empty.");
    }

    [Fact]
    public async Task CreateMembersEndPoint_Should_Return_Bad_Request_If_Request_Is_Missing_LastName()
    {
        var request =
            new CreateMemberRequest("Test", "", "Testgatan 2", "12345", "Testholm", "Test@test.se", "0734443322");

        var response = await Client.PostAsync("https://localhost:7298/api/member",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Last Name' must not be empty.");
    }

    [Fact]
    public async Task CreateMembersEndPoint_Should_Return_Bad_Request_If_Email_Is_Invalid_Email_Format()
    {
        var request = new CreateMemberRequest("Test", "Testsson", "Testgatan 2", "12345", "Testholm", "test.test.se",
            "0734443322");

        var response = await Client.PostAsync("https://localhost:7298/api/member",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<List<string>>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value!.FirstOrDefault().Should().Be("'Email' is not a valid email address.");
    }
}