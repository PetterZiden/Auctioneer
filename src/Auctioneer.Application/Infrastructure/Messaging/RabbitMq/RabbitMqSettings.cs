using RabbitMQ.Client;

namespace Auctioneer.Application.Infrastructure.Messaging.RabbitMq;

public static class RabbitMqSettings
{
    public static string AuctionExchange => "auction";
    public static string AuctionExchangeType => ExchangeType.Direct;
    public static string RateMemberQueue => "rate-member-email";
    public static string RateMemberRouteKey => "member";
    public static string PlaceBidQueue => "place-bid-email";
    public static string PlaceBidRouteKey => "bid";
}