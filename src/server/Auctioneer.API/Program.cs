using System.Threading.RateLimiting;
using Asp.Versioning;
using Auctioneer.API.OpenApi;
using Auctioneer.Application;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options => options.AddDefaultPolicy(
    policy => policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddApplication();
builder.Services.AddMediatr();
builder.Services.AddBackgroundWorkers();
builder.AddApplicationMetrics();
builder.AddApplicationAuth();
builder.AddInfrastructure();
builder.AddMessaging();

builder.Services.AddHttpContextAccessor();
builder.Services.AddRateLimiter(o => o
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(5);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));


builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
        options.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });
builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFeatureManagement(builder.Configuration.GetSection("FeatureFlags"))
    .AddFeatureFilter<PercentageFilter>();

builder.Configuration.AddUserSecrets<Program>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();

            options.SwaggerEndpoint(url, name);
        }
    });
//app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Auctioneer API v1"); });
}

app.UseCors();

app.UseHttpsRedirection();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

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