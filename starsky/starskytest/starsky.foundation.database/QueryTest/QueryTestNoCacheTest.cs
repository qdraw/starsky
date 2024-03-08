using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public sealed class QueryTestNoCacheTest
	{
		public QueryTestNoCacheTest()
		{
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase("QueryTestNoCacheTest");
			var options = builder.Options;
			var context = new ApplicationDbContext(options);
			var logger = new FakeIWebLogger();
			_query = new Query(context,new AppSettings(), null, logger);
		}

		private readonly Query _query;

		[TestMethod]
		public async Task QueryNoCache_SingleItem_Test()
		{
			await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "nocache.jpg",
				ParentDirectory = "/nocache",
				FileHash = "eruiopds",
				ColorClass = ColorClassParser.Color.Winner, // 1
				Tags = "",
				Title = ""
			});
            
			var singleItem = _query.SingleItem("/nocache/nocache.jpg")?.FileIndexItem;
			Assert.AreEqual("/nocache/nocache.jpg", singleItem?.FilePath);
		}

		[TestMethod]
		public void Query_IsCacheEnabled_False()
		{
			Assert.IsFalse(_query.IsCacheEnabled());
		}
	    
		[TestMethod]
		public void RemoveCacheItem_Disabled()
		{
			var updateStatusContent = new List<FileIndexItem>();
			_query.RemoveCacheItem(updateStatusContent);
			// it should not crash
			Assert.IsNotNull(updateStatusContent);
		}
		
	}
}
