using System.Collections.Generic;
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
	public class QueryTestNoCacheTest
	{
		public QueryTestNoCacheTest()
		{
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase("QueryTestNoCacheTest");
			var options = builder.Options;
			var context = new ApplicationDbContext(options);
			_logger = new FakeIWebLogger();
			_query = new Query(context,new AppSettings(), null, _logger);
		}

		private readonly Query _query;
		private readonly FakeIWebLogger _logger;

		[TestMethod]
		public void QueryNoCache_SingleItem_Test()
		{
			_query.AddItem(new FileIndexItem
			{
				FileName = "nocache.jpg",
				ParentDirectory = "/nocache",
				FileHash = "eruiopds",
				ColorClass = ColorClassParser.Color.Winner, // 1
				Tags = "",
				Title = ""
			});
            
			var singleItem = _query.SingleItem("/nocache/nocache.jpg").FileIndexItem;
			Assert.AreEqual("/nocache/nocache.jpg", singleItem.FilePath);
		}

		[TestMethod]
		public void Query_IsCacheEnabled_False()
		{
			Assert.AreEqual(false, _query.IsCacheEnabled());
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
