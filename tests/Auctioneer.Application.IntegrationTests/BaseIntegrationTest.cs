using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Auctioneer.Application.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<AuctioneerApiFactory>
{
    protected readonly ISender Sender;
    protected readonly IRepository<Entities.Member> MemberRepository;
    protected readonly IRepository<Auction> AuctionRepository;
    
    protected BaseIntegrationTest(AuctioneerApiFactory factory)
    {
        var scope = factory.Services.CreateScope();
        Sender = scope.ServiceProvider.GetRequiredService<ISender>();
        MemberRepository = scope.ServiceProvider.GetRequiredService<IRepository<Entities.Member>>();
        AuctionRepository = scope.ServiceProvider.GetRequiredService<IRepository<Auction>>();
    }
}