using Auctioneer.Application.Features.Members.Contracts;

namespace Auctioneer.API.IntegrationTests.Member;

[Collection("Auctioneer Test Collection")]
[TestCaseOrderer("Auctioneer.API.IntegrationTests.Helpers.PriorityOrderer", "Auctioneer.API.IntegrationTests")]
public class UpdateMemberTests(AuctioneerApiFactory factory) : BaseIntegrationTest(factory)
{
    [Fact, TestPriority(1)]
    public async Task UpdateMembersEndPoint_Should_Update_Members_If_Request_Is_Valid()
    {
        var memberId = await SeedMember();

        var request = new UpdateMemberRequest(memberId, "Test2", "Testsson2", "Testgatan 22", "54321", "Testborg",
            "0735554422");

        var response = await Client.PutAsync("https://localhost:7298/api/member",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var member = await MemberRepository.GetAsync(memberId);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        member?.FirstName.Should().Be(request.FirstName);
        member?.LastName.Should().Be(request.LastName);
        member?.Address.Street.Should().Be(request.Street);
        member?.Address.Zipcode.Should().Be(request.ZipCode);
        member?.Address.City.Should().Be(request.City);
        member?.PhoneNumber.Should().Be(request.PhoneNumber);
    }

    [Fact, TestPriority(2)]
    public async Task UpdateMembersEndPoint_Should_Update_Members_Address()
    {
        var memberId = await GetMemberId();

        var request = new UpdateMemberRequest(memberId, null, null, "Testgatan 23", "54323", "Testborg3", null);

        var response = await Client.PutAsync("https://localhost:7298/api/member",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        var member = await MemberRepository.GetAsync(memberId);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        member?.Address.Street.Should().Be(request.Street);
        member?.Address.Zipcode.Should().Be(request.ZipCode);
        member?.Address.City.Should().Be(request.City);
    }

    [Fact, TestPriority(0)]
    public async Task UpdateMembersEndPoint_Should_Update_Members_If_Member_Is_Not_Found()
    {
        var request = new UpdateMemberRequest(Guid.NewGuid(), "Test2", "Testsson2", "Testgatan 22", "54321", "Testborg",
            "0735554422");

        var response = await Client.PutAsync("https://localhost:7298/api/member",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("No member found");
    }

    [Fact, TestPriority(3)]
    public async Task UpdateMembersEndPoint_Should_Not_Update_Members_If_PhoneNumber_Is_Same_As_Before()
    {
        var memberId = await GetMemberId();

        var request = new UpdateMemberRequest(memberId, "Test2", "Testsson2", "Testgatan 22", "54321", "Testborg",
            "0734443322");

        var response = await Client.PutAsync("https://localhost:7298/api/member",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        var member = await MemberRepository.GetAsync(memberId);

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("Phone number can not be the same as current phone number");
        member?.PhoneNumber.Should().Be("0734443322");
    }

    [Fact, TestPriority(4)]
    public async Task UpdateMembersEndPoint_Should_Not_Update_Members_If_LastName_Is_Same_As_Before()
    {
        var memberId = await GetMemberId();

        var request = new UpdateMemberRequest(memberId, "Test2", "Testsson", "Testgatan 22", "54321", "Testborg",
            "0734443377");

        var response = await Client.PutAsync("https://localhost:7298/api/member",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        var member = await MemberRepository.GetAsync(memberId);

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("Last name can not be the same as current last name");
        member?.LastName.Should().Be("Testsson");
    }

    [Fact, TestPriority(5)]
    public async Task UpdateMembersEndPoint_Should_Not_Update_Members_If_FirstName_Is_Same_As_Before()
    {
        var memberId = await GetMemberId();

        var request = new UpdateMemberRequest(memberId, "Test", "Testsson2", "Testgatan 22", "54321", "Testborg",
            "0734443377");

        var response = await Client.PutAsync("https://localhost:7298/api/member",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"))
            .DeserializeResponseAsync<string>();

        var member = await MemberRepository.GetAsync(memberId);

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Value.Should().NotBeNull();
        response.Value.Should().Be("First name can not be the same as current first name");
        member?.FirstName.Should().Be("Test");
    }
}