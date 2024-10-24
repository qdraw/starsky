using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.database.Thumbnails;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Thumbnails;

[TestClass]
public sealed class ThumbnailQueryFactoryTest
{
	[TestMethod]
	public void QueryFactoryTest_Null()
	{
		var factory = new ThumbnailQueryFactory(
			null, null,
			null, null!, null!);
		var query = factory.ThumbnailQuery();
		Assert.IsNull(query);
	}

	[TestMethod]
	public void QueryFactoryTest_QueryReturn()
	{
		var queryFactory = new ThumbnailQueryFactory(null, null,
			new ThumbnailQuery(null!, null, new FakeIWebLogger(), new FakeMemoryCache()),
			new FakeIWebLogger(), new FakeMemoryCache());
		var query = queryFactory.ThumbnailQuery();
		Assert.AreEqual(typeof(ThumbnailQuery), query!.GetType());
	}

	[TestMethod]
	public void QueryFactoryTest_QueryReturn2()
	{
		var services = new ServiceCollection();
		var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
		builderDb.UseInMemoryDatabase("test1234");
		var options = builderDb.Options;
		var queryFactory = new ThumbnailQueryFactory(
			new SetupDatabaseTypes(
				new AppSettings { DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase },
				services), null,
			new ThumbnailQuery(new ApplicationDbContext(options), null, new FakeIWebLogger(),
				new FakeMemoryCache()),
			new FakeIWebLogger(), new FakeMemoryCache());
		var query = queryFactory.ThumbnailQuery();
		Assert.AreEqual(typeof(ThumbnailQuery), query!.GetType());
	}

	[TestMethod]
	public async Task QueryFactoryTest_FakeIQueryReturn()
	{
		var fakeIQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new("test3", null, null, null, null)
		});

		var queryFactory = new ThumbnailQueryFactory(null, null, fakeIQuery,
			new FakeIWebLogger(), new FakeMemoryCache());
		var query = queryFactory.ThumbnailQuery();


		var resultFakeIQuery = query as FakeIThumbnailQuery;
		var count = ( await resultFakeIQuery!.Get("test3") ).Count;
		Assert.AreEqual(1, count);
	}


	[TestMethod]
	public async Task QueryFactoryTest_FakeIQuery_IgnoreNoItemsInList()
	{
		var fakeIQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new("test5", null, null, null, null)
		});

		var queryFactory = new ThumbnailQueryFactory(null, null, fakeIQuery,
			new FakeIWebLogger(), new FakeMemoryCache());

		var query = queryFactory.ThumbnailQuery();


		var resultFakeIQuery = query as FakeIThumbnailQuery;
		var count = ( await resultFakeIQuery!.Get("test1") ).Count;
		Assert.AreEqual(0, count);
	}
}
