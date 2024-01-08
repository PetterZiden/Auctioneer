using Auctioneer.Application.Infrastructure.Persistence;
using Auctioneer.Application.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;

namespace Auctioneer.API.IntegrationTests;

public class AuctioneerApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private static MongoDbRunner _runner;

    public async Task InitializeAsync()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: true);
    }

    public new async Task DisposeAsync()
    {
        _runner.Dispose();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.Single(s => s.ImplementationType == typeof(OutboxPublisher));
            services.Remove(descriptor);

            services.Configure<AuctioneerDatabaseSettings>(opts =>
            {
                opts.ConnectionString = _runner.ConnectionString;
            });
        });
    }
}