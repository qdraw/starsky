using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.platform.Extensions;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class MemoryCacheDebugControllerTest
	{
		[TestMethod]
		public void CatchFakeCacheDebug()
		{
			var controller = new MemoryCacheDebugController(new FakeMemoryCache());
			controller.MemoryCacheDebug();
		}
		
		[TestMethod]
		public void CacheDebug()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache();

			var buildServiceProvider =  provider.BuildServiceProvider();
			var memoryCache = buildServiceProvider.GetService<IMemoryCache>();
			memoryCache.Set("test", "");
			
			var controller = new MemoryCacheDebugController(memoryCache);
			var actionResult = controller.MemoryCacheDebug() as JsonResult;
			var list = actionResult.Value as Dictionary<string, object>;
			Assert.AreEqual(1, list.Count);
		}
	}
}
