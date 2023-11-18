namespace Auctioneer.Application.Common.Interfaces;

public interface ICacheableMediatrQuery
{
    bool BypassCache { get; }
    string CacheKey { get; }
    TimeSpan? EvictionExpirationInMinutes { get; }
}