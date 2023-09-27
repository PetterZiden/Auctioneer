namespace Auctioneer.Application.Infrastructure.Persistence;

public class AuctioneerDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string MemberCollectionName { get; set; } = null!;
    
    public string AuctionCollectionName { get; set; } = null!;
}