using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace VoltProjects.Tests.Utils.Memory;

public sealed class NullMemoryCache : IMemoryCache
{
    public ICacheEntry CreateEntry(object key)
    {
        return new NullCacheEntry { Key = key };
    }

    public void Dispose()
    {            
    }

    public void Remove(object key)
    {
    }

    public bool TryGetValue(object key, out object? value)
    {
        value = null;
        return false;
    }

    private sealed class NullCacheEntry : ICacheEntry
    {
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        public IList<IChangeToken> ExpirationTokens { get; set; }

        public object Key { get; set; }

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; set; }

        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public object Value { get; set; }

        public void Dispose()
        {
        }
    }
}