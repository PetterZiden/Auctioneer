using Auctioneer.EmailService;
using Auctioneer.EmailService.Interfaces;
using Auctioneer.EmailService.Services;
using Auctioneer.EmailService.Settings;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;
        services.Configure<CloudAmqpSettings>(configuration.GetSection("CloudAMQP"));
        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        services.AddSingleton<IMessageHandlerService, MessageHandlerService>();
        services.AddHostedService<Worker>();
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(hostingContext.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();
        logging.ClearProviders();
        logging.AddSerilog(logger);
    })
    .Build();

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

host.Run();