using Microsoft.Extensions.Caching.Memory;

namespace EbayClone.MVC.Services;

public sealed class ApiCacheService(IMemoryCache memoryCache) : IApiCache
{
    public bool TryGet<T>(string key, out T? value) => memoryCache.TryGetValue(key, out value);

    public void Set<T>(string key, T? value, TimeSpan lifetime) =>
        memoryCache.Set(key, value, lifetime);
}
