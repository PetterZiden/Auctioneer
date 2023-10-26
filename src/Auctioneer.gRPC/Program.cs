using System.Threading.RateLimiting;
using Auctioneer.Application;
using Auctioneer.gRPC.Services;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();
builder.AddApplicationAuth();
builder.Services.AddApplication();

builder.Services.AddGrpc();

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

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

app.MapGrpcService<MemberService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();