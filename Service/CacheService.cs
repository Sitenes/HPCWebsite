using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    public CacheService(IMemoryCache cache,ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(_cache.Get<T>(key));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiry.HasValue)
            options.SetAbsoluteExpiration(expiry.Value);

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }
    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        try
        {
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                if (expiration.HasValue)
                {
                    entry.AbsoluteExpirationRelativeToNow = expiration;
                }
                else
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                }

                return await factory();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting or creating cache for key {key}");
            return await factory();
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing cache for key {key}");
            return Task.CompletedTask;
        }
    }
    public Task ClearAllAsync()
    {
        // Note: This is a simplified implementation. 
        // In production, you might need a more sophisticated approach
        // or use distributed cache with proper clear functionality
        if (_cache is MemoryCache memoryCache)
        {
            memoryCache.Compact(1.0);
        }

        return Task.CompletedTask;
    }
}