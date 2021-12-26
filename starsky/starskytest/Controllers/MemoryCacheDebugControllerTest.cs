using System.Collections.Generic;
using System.Dynamic;
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
			var controller = new MemoryCacheDebugController(new FakeMemoryCache(), new FakeIWebLogger());
			var actionResult = controller.MemoryCacheDebug() as JsonResult;
			var list = actionResult.Value as Dictionary<string, object>;
			Assert.AreEqual(0, list.Count);
		}
		
		[TestMethod]
		public void CacheDebug()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache();

			var buildServiceProvider =  provider.BuildServiceProvider();
			var memoryCache = buildServiceProvider.GetService<IMemoryCache>();
			memoryCache.Set("test", "");
			
			var controller = new MemoryCacheDebugController(memoryCache, new FakeIWebLogger());
			var actionResult = controller.MemoryCacheDebug() as JsonResult;
			var list = actionResult.Value as Dictionary<string, object>;
			Assert.AreEqual(1, list.Count);
		}
		
		[TestMethod]
		public void CacheDebug_NonValidJson()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache();

			var buildServiceProvider =  provider.BuildServiceProvider();
			var memoryCache = buildServiceProvider.GetService<IMemoryCache>();

			dynamic nonValidData = new ExpandoObject();
			nonValidData.Data = nonValidData; //  A possible object cycle was detected

			memoryCache.Set("test", (object)nonValidData);
			
			var controller = new MemoryCacheDebugController(memoryCache, new FakeIWebLogger());
			var actionResult = controller.MemoryCacheDebug() as JsonResult;
			var list = actionResult.Value as Dictionary<string, object>;
			Assert.AreEqual(1, list.Count);
			list.TryGetValue("test", out var result);
			var stringValue = ( string ) result;
			
			Assert.IsTrue(stringValue.StartsWith("[ERROR]") );
		}
	}
}
