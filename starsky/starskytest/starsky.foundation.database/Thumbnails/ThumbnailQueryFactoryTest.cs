using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.database.Thumbnails;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Thumbnails
{
	[TestClass]
	public sealed class ThumbnailQueryFactoryTest
	{
		[TestMethod]
		public void QueryFactoryTest_Null()
		{
			var query = new ThumbnailQueryFactory(null,null,null).ThumbnailQuery();
			Assert.IsNull(query);
		}
		
		[TestMethod]
		public void QueryFactoryTest_QueryReturn()
		{
			var queryFactory = new ThumbnailQueryFactory(null, new ThumbnailQuery(null),
				new FakeIWebLogger());
			var query = queryFactory.ThumbnailQuery();
			Assert.AreEqual(typeof(ThumbnailQuery),query!.GetType());
		}
		
		[TestMethod]
		public async Task QueryFactoryTest_FakeIQueryReturn()
		{
			var fakeIQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>{new ThumbnailItem("test3")});
			
			var queryFactory = new ThumbnailQueryFactory(null, fakeIQuery,
				new FakeIWebLogger());
			var query = queryFactory.ThumbnailQuery();
			
			
			var resultFakeIQuery = query as FakeIThumbnailQuery;
			var count = (await resultFakeIQuery!.Get("test3")).Count;
			Assert.AreEqual(1, count);
		}
		
		
		
		[TestMethod]
		public async Task QueryFactoryTest_FakeIQuery_IgnoreNoItemsInList()
		{
			var fakeIQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>{new ThumbnailItem("test5")});
			
			var queryFactory = new ThumbnailQueryFactory(null, fakeIQuery,
				new FakeIWebLogger());
			
			var query = queryFactory.ThumbnailQuery();
			
			
			var resultFakeIQuery = query as FakeIThumbnailQuery;
			var count = (await resultFakeIQuery!.Get("test1")).Count;
			Assert.AreEqual(0, count);
		}
	}

}

