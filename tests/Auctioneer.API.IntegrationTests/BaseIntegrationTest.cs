using System.Net.Http.Headers;
using Auctioneer.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Auctioneer.API.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<AuctioneerApiFactory>
{
    protected readonly IRepository<Application.Entities.Member> MemberRepository;
    protected readonly IRepository<Application.Entities.Auction> AuctionRepository;
    protected readonly IUnitOfWork UnitOfWork;
    protected HttpClient Client { get; }

    protected BaseIntegrationTest(AuctioneerApiFactory factory)
    {
        var scope = factory.Services.CreateScope();
        MemberRepository = scope.ServiceProvider.GetRequiredService<IRepository<Application.Entities.Member>>();
        AuctionRepository = scope.ServiceProvider.GetRequiredService<IRepository<Application.Entities.Auction>>();
        UnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
}