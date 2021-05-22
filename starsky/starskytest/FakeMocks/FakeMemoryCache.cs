using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.database.Models;
using starskycore.Models;
using starskycore.ViewModels;
using starskytest.FakeCreateAn;

namespace starskytest.FakeMocks
{
	public class FakeMemoryCache : IMemoryCache
	{
		private readonly ICacheEntry _fakeCacheEntry;
		private readonly Dictionary<string, object> _items;

		public FakeMemoryCache(Dictionary<string, object> items)
		{
			var services = new ServiceCollection();
			services.AddSingleton<ICacheEntry,FakeICacheEntry>();
			var serviceProvider = services.BuildServiceProvider();
			_fakeCacheEntry = serviceProvider.GetRequiredService<ICacheEntry>();
			_items = items;
		}
		public void Dispose()
		{
		}

		public bool TryGetValue(object key, out object value)
		{
			value = _items.FirstOrDefault(p => Equals(p.Key, key)).Value;
	        
			return value != null;
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
