using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Helpers;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.metaupdate.Helpers
{
	[TestClass]
	public class AddParentCacheIfNotExistTest
	{
		
		[TestMethod]
		public async Task AddParentCacheIfNotExist_ignore_nothing()
		{
			var element = new AddParentCacheIfNotExist(new FakeIQuery(
				new List<FileIndexItem>{new FileIndexItem("/test.jpg")}), new FakeIWebLogger());

			var result = await element.AddParentCacheIfNotExistAsync(
				new List<string>());
			Assert.AreEqual(0,result.Count);
		}
		
		[TestMethod]
		public async Task AddParentCacheIfNotExist_TriggerCacheSync()
		{
			var fakeContent =
				new List<FileIndexItem> {new FileIndexItem("/test.jpg")};

			var fakeQuery = new FakeIQuery(fakeContent);
			var metaPreflight = new AddParentCacheIfNotExist(fakeQuery, new FakeIWebLogger());

			await metaPreflight.AddParentCacheIfNotExistAsync(
				fakeContent.Select(p => p.FilePath));

			var (_, cacheGetParentFolder) = fakeQuery.CacheGetParentFolder("/");
			
			Assert.AreEqual(1,cacheGetParentFolder.Count);
		}
		
		[TestMethod]
		public async Task AddParentCacheIfNotExist_IgnoreWhenCacheExists()
		{
			var fakeContent =
				new List<FileIndexItem> {new FileIndexItem("/test.jpg"){FileHash = "test1"}};
			var fakeContentCache =
				new List<FileIndexItem> {new FileIndexItem("/test.jpg"){FileHash = "__old_key__"}};
			
			var fakeQuery = new FakeIQuery(fakeContent,fakeContentCache);
			var metaPreflight = new AddParentCacheIfNotExist(fakeQuery, new FakeIWebLogger());

			await metaPreflight.AddParentCacheIfNotExistAsync(
				fakeContent.Select(p => p.FilePath));

			var (_, cacheGetParentFolder) = fakeQuery.CacheGetParentFolder("/");
			
			Assert.AreEqual(1,cacheGetParentFolder.Count);
			Assert.AreEqual("__old_key__",cacheGetParentFolder[0].FileHash);
		}
	}
}
