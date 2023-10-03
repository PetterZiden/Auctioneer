using Auctioneer.EmailService;
using Auctioneer.EmailService.Interfaces;
using Auctioneer.EmailService.Services;
using Auctioneer.EmailService.Settings;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        services.Configure<CloudAmqpSettings>(configuration.GetSection("CloudAMQP"));
        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        services.AddHostedService<Worker>();
    })
    .Build();

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

host.Run();