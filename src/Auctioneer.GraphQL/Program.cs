using Auctioneer.Application;
using Auctioneer.GraphQL.Auctions;
using Auctioneer.GraphQL.Members;

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

app.MapGraphQL();

app.Run();