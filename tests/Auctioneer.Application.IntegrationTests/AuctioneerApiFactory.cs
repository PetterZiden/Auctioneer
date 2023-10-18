using Auctioneer.API;
using Auctioneer.Application.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;

namespace Auctioneer.Application.IntegrationTests;

public class AuctioneerApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private static MongoDbRunner _runner;
    
    public async Task InitializeAsync()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: true);
    }

    public new async Task DisposeAsync()
    {
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.Configure<AuctioneerDatabaseSettings>(opts =>
            {
                opts.ConnectionString = _runner.ConnectionString;
            });
        });
    }
}