using System.Threading.RateLimiting;
using Auctioneer.Application;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options => options.AddDefaultPolicy(
    policy => policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddApplication();
builder.Services.AddMediatr();
builder.Services.AddBackgroundWorkers();
builder.AddApplicationAuth();
builder.AddInfrastructure();
builder.AddMessaging();

builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auctioneer API", Version = "v1" }));

builder.Configuration.AddUserSecrets<Program>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Auctioneer API v1"); });


app.UseCors();

app.UseHttpsRedirection();

app.UseExceptionHandler(app.Environment.IsDevelopment() ? "/error-development" : "/error");

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

app.MapControllers();

app.Run();