using Auctioneer.Application;
using Auctioneer.gRPC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();
builder.AddApplicationAuth();
builder.Services.AddApplication();

builder.Services.AddGrpc();

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<MemberService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();