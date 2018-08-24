﻿using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.Models;

namespace starskytests.FakeMocks
{
    public class FakeMemoryCache :IMemoryCache
    {
        private readonly ICacheEntry _fakeCacheEntry;

        public FakeMemoryCache()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ICacheEntry,FakeICacheEntry>();
            var serviceProvider = services.BuildServiceProvider();
            _fakeCacheEntry = serviceProvider.GetRequiredService<ICacheEntry>();
        }
        public void Dispose()
        {
        }

        public bool TryGetValue(object key, out object value)
        {
            value = new FileIndexItem{Tags = "test"};
            // this item does never exist in cache :)
            if (key.ToString() == "info_" + new CreateAnImage().FullFilePath) return false;
            return true ;
        }

        public ICacheEntry CreateEntry(object key)
        {
            return _fakeCacheEntry;
        }

        public void Remove(object key)
        {
        }
    }
}