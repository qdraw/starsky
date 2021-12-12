using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Extensions;

namespace starskytest.starsky.foundation.platform.Extensions
{
	[TestClass]
	public class MemoryCacheExtensionsTest
	{
		[TestMethod]
		public void NoContentInCache()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache();

			var buildServiceProvider =  provider.BuildServiceProvider();
			var memoryCache = buildServiceProvider.GetService<IMemoryCache>();
			var keys= memoryCache.GetKeys<string>();
			Assert.AreEqual(0, keys.Count());
		}
		
		[TestMethod]
		public void OneItemInCache()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache();

			var buildServiceProvider =  provider.BuildServiceProvider();
			var memoryCache = buildServiceProvider.GetService<IMemoryCache>();
			memoryCache.Set("test", "");
			var keys= memoryCache.GetKeys<string>().ToList();
			Assert.AreEqual(1, keys.Count);
			Assert.AreEqual("test", keys[0]);
		}
		
	}
}
