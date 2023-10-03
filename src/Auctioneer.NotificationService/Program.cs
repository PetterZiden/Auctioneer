using MassTransit;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumers(typeof(Program).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetSection("CloudAMQP:Url").Value);

                cfg.ConfigureEndpoints(context);
                cfg.UseMessageRetry(r => { r.Interval(3, TimeSpan.FromSeconds(5)); });
            });
        });
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