using System.Threading.RateLimiting;
using Auctioneer.Application;
using Auctioneer.GraphQL.Auctions;
using Auctioneer.GraphQL.Members;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();
builder.AddApplicationAuth();
builder.Services.AddApplication();

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType(x => x.Name("Query"))
    .AddType<MemberQueries>()
    .AddType<AuctionQueries>()
    .AddMutationType(x => x.Name("Mutation"))
    .AddType<MemberMutations>()
    .AddType<AuctionMutations>();


var app = builder.Build();

app.UseRouting();

app.UseAuthentication();

app.UseRateLimiter(new RateLimiterOptions
{
    GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
    {
        return RateLimitPartition.GetConcurrencyLimiter<string>("GeneralLimit",
            _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = 1,
                QueueLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.NewestFirst
            });
    }),
    RejectionStatusCode = 429
});

app.MapGraphQL();

app.Run();