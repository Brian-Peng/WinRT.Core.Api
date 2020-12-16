using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinRT.Core.Common.MemoryCache
{
    /// <summary>
    ///  In-memory cache,将数据获取到以后，定义缓存，然后在其他地方使用的时候，在根据key去获取当前数据，然后再操作等等
    /// </summary>
    public class MemoryCaching : ICaching
    {
        private IMemoryCache _cache;
        public MemoryCaching(IMemoryCache cache)
        {
            _cache = cache;
        }
        public object Get(string cacheKey)
        {
            return _cache.Get(cacheKey);
        }

        public void Set(string cacheKey, object cacheValue)
        {
            _cache.Set(cacheKey, cacheValue, TimeSpan.FromSeconds(7200));
        }
    }
}
