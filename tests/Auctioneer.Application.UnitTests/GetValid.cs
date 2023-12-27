using Auctioneer.Application.Entities;

namespace Auctioneer.Application.UnitTests;

public static class GetValid
{
    public static Member Member() =>
        Entities.Member.Create("Test", "Testsson", "test@test.se", "0734443322", "Testgatan 2", "12345", "Testholm")
            .Value;


    public static Auction Auction() =>
        Entities.Auction.Create(Guid.NewGuid(), "TestAuction", "MockDescription", DateTimeOffset.Now.AddHours(2),
            DateTimeOffset.Now.AddDays(7), 100, "../images/test.png").Value;
}