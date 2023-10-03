using System.Reflection;
using Auctioneer.Application.Common.Behaviours;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions;
using Auctioneer.Application.Features.Members;
using Auctioneer.Application.Infrastructure.Messaging.MassTransit;
using Auctioneer.Application.Infrastructure.Messaging.RabbitMq;
using Auctioneer.Application.Infrastructure.Persistence;
using MassTransit;
using MassTransit.RabbitMqTransport.Topology;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Serilog;

namespace Auctioneer.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        services.AddSingleton<IRepository<Member>, MemberRepository>();
        services.AddSingleton<IRepository<Auction>, AuctionRepository>();

        services.AddScoped<IMessageProducer, RabbitMqProducer>();
        services.AddScoped<INotificationProducer, MassTransitProducer>();

        return services;
    }

    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AuctioneerDatabaseSettings>(
            builder.Configuration.GetSection("AuctioneerDatabaseSettings"));

        builder.Services.Configure<CacheSettings>(
            builder.Configuration.GetSection("CacheSettings"));

        builder.Services.Configure<CloudAmqpSettings>(
            builder.Configuration.GetSection("CloudAMQP"));

        builder.Services.AddMemoryCache();

        builder.Services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(builder.Configuration.GetSection("CloudAMQP:Url").Value);

                cfg.ConfigureEndpoints(context);
                cfg.UseMessageRetry(r => { r.Interval(3, TimeSpan.FromSeconds(5)); });
            });
        });

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);

        return builder;
    }
}