using System.Text;
using Auctioneer.Application.Auth;
using Auctioneer.Application.Common;
using Auctioneer.Application.Common.Behaviours;
using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Entities;
using Auctioneer.Application.Features.Auctions;
using Auctioneer.Application.Features.Members;
using Auctioneer.Application.Infrastructure.Messaging.MassTransit;
using Auctioneer.Application.Infrastructure.Messaging.RabbitMq;
using Auctioneer.Application.Infrastructure.Persistence;
using Auctioneer.Application.Infrastructure.Services;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Auctioneer.Application;

public static class ConfigureServices
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IRepository<Member>, MemberRepository>();
        services.AddSingleton<IRepository<Auction>, AuctionRepository>();
        services.AddSingleton<IRepository<DomainEvent>, EventRepository>();
        services.AddSingleton<IUnitOfWork, UnitOfWork>();
    }

    public static void AddApplicationAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb")));

        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password = new PasswordOptions
                {
                    RequireNonAlphanumeric = false,
                    RequireLowercase = false,
                    RequireUppercase = false,
                    RequireDigit = false
                };
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
                };
            });
    }

    public static void AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AuctioneerDatabaseSettings>(
            builder.Configuration.GetSection("AuctioneerDatabaseSettings"));

        builder.Services.Configure<CacheSettings>(
            builder.Configuration.GetSection("CacheSettings"));

        builder.Services.Configure<CloudAmqpSettings>(
            builder.Configuration.GetSection("CloudAMQP"));

        builder.Services.AddMemoryCache();

        builder.Services.AddSingleton<IDomainEventService, DomainEventService>();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(logger);
    }

    public static void AddMediatr(this IServiceCollection services)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembly));
        }

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
    }

    public static void AddMessaging(this WebApplicationBuilder builder)
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
    }

    public static void AddBackgroundWorkers(this IServiceCollection services)
    {
        services.AddHostedService<OutboxPublisher>();
    }
}