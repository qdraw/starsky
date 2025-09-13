using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.metaupdate.Helpers;

[TestClass]
public sealed class AddParentCacheIfNotExistTest
{
	[TestMethod]
	public async Task AddParentCacheIfNotExist_ignore_nothing()
	{
		var element = new AddParentCacheIfNotExist(new FakeIQuery(
			new List<FileIndexItem> { new("/test.jpg") }), new FakeIWebLogger());

		var result = await element.AddParentCacheIfNotExistAsync(
			new List<string>());
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task AddParentCacheIfNotExist_TriggerCacheSync()
	{
		var fakeContent =
			new List<FileIndexItem> { new("/test.jpg") };

		var fakeQuery = new FakeIQuery(fakeContent);
		var metaPreflight = new AddParentCacheIfNotExist(fakeQuery, new FakeIWebLogger());

		await metaPreflight.AddParentCacheIfNotExistAsync(
			fakeContent.Select(p => p.FilePath!));

		var (_, cacheGetParentFolder) = fakeQuery.CacheGetParentFolder("/");

		Assert.HasCount(1, cacheGetParentFolder);
	}

	[TestMethod]
	public async Task AddParentCacheIfNotExist_IgnoreWhenCacheExists()
	{
		var fakeContent =
			new List<FileIndexItem> { new("/test.jpg") { FileHash = "test1" } };
		var fakeContentCache =
			new List<FileIndexItem> { new("/test.jpg") { FileHash = "__old_key__" } };

		var fakeQuery = new FakeIQuery(fakeContent, fakeContentCache);
		var metaPreflight = new AddParentCacheIfNotExist(fakeQuery, new FakeIWebLogger());

		await metaPreflight.AddParentCacheIfNotExistAsync(
			fakeContent.Select(p => p.FilePath!));

		var (_, cacheGetParentFolder) = fakeQuery.CacheGetParentFolder("/");

		Assert.HasCount(1, cacheGetParentFolder);
		Assert.AreEqual("__old_key__", cacheGetParentFolder[0].FileHash);
	}
}
