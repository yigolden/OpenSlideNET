using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;

namespace MultiSlideServer.Cache
{
    public class DeepZoomGeneratorCache
    {
        private MemoryCache _cache;

        public DeepZoomGeneratorCache()
        {
            _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        }

        private static void ItemEvicted(object key, object value, EvictionReason reason, object state)
        {
            (value as IDisposable)?.Dispose();
        }

        public bool TryGet(string name, out RetainableDeepZoomGenerator dz)
        {
            return _cache.TryGetValue(name, out dz);
        }

        public bool TrySet(string name, RetainableDeepZoomGenerator dz)
        {
            var cacheEntryOption = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(3)
            };
            cacheEntryOption.RegisterPostEvictionCallback(ItemEvicted);
            RetainableDeepZoomGenerator cachedDz = _cache.Set(name, dz, cacheEntryOption);
            return ReferenceEquals(cachedDz, dz);
        }
    }
}
