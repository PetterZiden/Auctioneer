using System.Net.Http.Headers;
using Auctioneer.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Auctioneer.API.IntegrationTests;

public abstract class BaseIntegrationTest
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

    protected async Task ResetDb()
    {
        var members = await MemberRepository.GetAsync();
        foreach (var member in members)
        {
            await MemberRepository.DeleteAsync(member.Id, CancellationToken.None);
        }

        var auctions = await AuctionRepository.GetAsync();
        foreach (var auction in auctions)
        {
            await AuctionRepository.DeleteAsync(auction.Id, CancellationToken.None);
        }

        await UnitOfWork.SaveAsync();
    }


    protected async Task<Guid> GetMemberId()
    {
        var members = await MemberRepository.GetAsync();
        var memberId = members.FirstOrDefault(a => a.FirstName == "Test")?.Id;
        return memberId ?? Guid.Empty;
    }

    protected async Task<(Guid, Guid)> SeedMembers()
    {
        var member = Application.Entities.Member.Create(
            "Test",
            "Testsson",
            "test@test.se",
            "0734443322",
            "testgatan 2",
            "12345",
            "testholm").Value;

        var member2 = Application.Entities.Member.Create(
            "Test2",
            "Testsson2",
            "test2@test.se",
            "0734443311",
            "testgatan 22",
            "12341",
            "testholm").Value;

        await MemberRepository.CreateAsync(member, new CancellationToken());
        await MemberRepository.CreateAsync(member2, new CancellationToken());
        await UnitOfWork.SaveAsync();

        return (member.Id, member2.Id);
    }

    protected async Task<Guid> SeedMember()
    {
        var member = Application.Entities.Member.Create(
            "Test",
            "Testsson",
            "test@test.se",
            "0734443322",
            "testgatan 2",
            "12345",
            "testholm").Value;

        await MemberRepository.CreateAsync(member, new CancellationToken());
        await UnitOfWork.SaveAsync();

        return member.Id;
    }

    protected async Task<Guid> GetAuctionId()
    {
        var auctions = await AuctionRepository.GetAsync();
        var auctionId = auctions.FirstOrDefault(a => a.Title == "TestAuction")?.Id;
        return auctionId ?? Guid.Empty;
    }

    protected async Task SeedAuctions()
    {
        var auction = Application.Entities.Auction.Create(
            Guid.NewGuid(),
            "TestAuction",
            "TestDescription",
            DateTimeOffset.Now.AddHours(6),
            DateTimeOffset.Now.AddDays(7),
            100,
            "../images.test.jpg").Value;

        var auction2 = Application.Entities.Auction.Create(
            Guid.NewGuid(),
            "TestAuction2",
            "TestDescription2",
            DateTimeOffset.Now.AddHours(6),
            DateTimeOffset.Now.AddDays(7),
            150,
            "../images.test2.jpg").Value;

        await AuctionRepository.CreateAsync(auction, new CancellationToken());
        await AuctionRepository.CreateAsync(auction2, new CancellationToken());
        await UnitOfWork.SaveAsync();
    }

    protected async Task<Guid> SeedAuction()
    {
        var auction = Application.Entities.Auction.Create(
            Guid.NewGuid(),
            "TestAuction",
            "TestDescription",
            DateTimeOffset.Now.AddHours(6),
            DateTimeOffset.Now.AddDays(7),
            100,
            "../images.test.jpg").Value;

        await AuctionRepository.CreateAsync(auction, new CancellationToken());
        await UnitOfWork.SaveAsync();

        return auction.Id;
    }

    protected async Task<Guid> SeedAuction(Guid memberId)
    {
        var auction = Application.Entities.Auction.Create(
            memberId,
            "TestAuction",
            "TestDescription",
            DateTimeOffset.Now.AddHours(6),
            DateTimeOffset.Now.AddDays(7),
            100,
            "../images.test.jpg").Value;

        await AuctionRepository.CreateAsync(auction, new CancellationToken());
        await UnitOfWork.SaveAsync();

        return auction.Id;
    }
}