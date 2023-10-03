using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Auctioneer.Application.Infrastructure.Messaging.RabbitMq;

public sealed class RabbitMqChannelFactory
{
    private readonly IConfiguration _configuration;

    private static readonly Lazy<RabbitMqChannelFactory> instance = new(() => new RabbitMqChannelFactory());

    public static RabbitMqChannelFactory Instance => instance.Value;

    private IConnection _connection;
    private IModel _channel;

    private RabbitMqChannelFactory()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("../Auctioneer.API/appsettings.json", optional: true);

        _configuration = builder.Build();

        Initialize();
    }

    private void Initialize()
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(_configuration["CloudAMQP:Url"])
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public IModel GetChannel()
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        Initialize();
        return _channel;
    }
}