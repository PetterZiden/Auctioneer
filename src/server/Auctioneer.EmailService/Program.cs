using Auctioneer.EmailService;
using Auctioneer.EmailService.Interfaces;
using Auctioneer.EmailService.Services;
using Auctioneer.EmailService.Settings;
using Serilog;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.Configure<CloudAmqpSettings>(config.GetSection("CloudAMQP"));
        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        services.AddSingleton<IMessageHandlerService, MessageHandlerService>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddHostedService<Worker>();
        services
            .AddFluentEmail("Auctioneer@test.test")
            .AddRazorRenderer();
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

host.Run();