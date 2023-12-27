using Auctioneer.Application.Common.Interfaces;
using Auctioneer.Application.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Auctioneer.Application.Common.Behaviours;

public class CachingBehavior<TRequest, TResponse>(
    IMemoryCache cache,
    ILogger<TRequest> logger,
    IOptions<CacheSettings> settings)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableMediatrQuery
{
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly ILogger<TRequest> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly CacheSettings _settings = settings.Value;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableMediatrQuery cacheableQuery) return await next();

        TResponse response;
        if (cacheableQuery.BypassCache) return await next();

        if (_cache.TryGetValue(request.CacheKey, out TResponse cachedResponse) && cachedResponse is not null)
        {
            response = cachedResponse;
        }
        else
        {
            response = await GetResponseAndAddToCache();
        }

        return response;

        async Task<TResponse> GetResponseAndAddToCache()
        {
            response = await next();
            _cache.Set(cacheableQuery.CacheKey, response,
                cacheableQuery.EvictionExpirationInMinutes ??
                TimeSpan.FromMinutes(_settings.EvictionExpirationInMinutes));
            return response;
        }
    }
}