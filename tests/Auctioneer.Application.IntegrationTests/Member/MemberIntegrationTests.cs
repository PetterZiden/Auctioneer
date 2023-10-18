using Auctioneer.Application.Features.Members.Commands;
using Auctioneer.Application.Features.Members.Queries;

namespace Auctioneer.Application.IntegrationTests.Member;

public class MemberIntegrationTests : BaseIntegrationTest
{
    public MemberIntegrationTests(AuctioneerApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateMemberCommand_Should_Add_New_Member_To_Database()
    {
        var command = new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = "Testsson",
            Street = "Testgatan 22",
            ZipCode = "12345",
            City = "Testholm",
            Email = "Test@test.se",
            PhoneNumber = "0704443322"
        };

        var memberId = await Sender.Send(command);


        var member = await MemberRepository.GetAsync(memberId.Value);
        Assert.NotNull(member);
    }

    [Fact]
    public async Task GetMemberQuery_Should_Fetch_Member_If_Member_Exist()
    {
        var command = new CreateMemberCommand
        {
            FirstName = "Test",
            LastName = "Testsson",
            Street = "Testgatan 22",
            ZipCode = "12345",
            City = "Testholm",
            Email = "Test@test.se",
            PhoneNumber = "0704443322"
        };

        var memberId = await Sender.Send(command);

        var query = new GetMemberQuery
        {
            Id = memberId.Value
        };

        var member = await Sender.Send(query);

        Assert.NotNull(member);
    }
}