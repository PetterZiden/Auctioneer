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
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Auctioneer.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IRepository<Member>, MemberRepository>();
        services.AddSingleton<IRepository<Auction>, AuctionRepository>();
        services.AddSingleton<IUnitOfWork, UnitOfWork>();

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

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);

        return builder;
    }

    public static IServiceCollection AddMediatr(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        return services;
    }

    public static WebApplicationBuilder AddMessaging(this WebApplicationBuilder builder)
    {
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

        builder.Services.AddScoped<IMessageProducer, RabbitMqProducer>();
        builder.Services.AddScoped<INotificationProducer, MassTransitProducer>();

        return builder;
    }
}