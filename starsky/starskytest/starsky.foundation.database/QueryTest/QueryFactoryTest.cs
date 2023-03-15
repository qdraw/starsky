using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public sealed class QueryFactoryTest
	{
		[TestMethod]
		public void QueryFactoryTest_Null()
		{
			var query = new QueryFactory(null,
				null,null,null,null, null).Query();
			Assert.IsNull(query);
		}
		
		[TestMethod]
		public void QueryFactoryTest_QueryReturn()
		{
			var query = new QueryFactory(null,new Query(null,null, 
				null, new FakeIWebLogger()),null,
				null,null, null).Query();
			Assert.AreEqual(typeof(Query),query.GetType());
		}
		
		[TestMethod]
		public void QueryFactoryTest_FakeIQueryReturn()
		{
			var fakeIQuery = new FakeIQuery(new List<FileIndexItem>{new FileIndexItem("/test.jpg")});
			var query = new QueryFactory(null,fakeIQuery,null,
				null,null, null).Query();
			
			var resultFakeIQuery = query as FakeIQuery;
			Assert.AreEqual(1, resultFakeIQuery?.GetAllRecursive().Count);
			Assert.AreEqual("/test.jpg", resultFakeIQuery?.GetAllRecursive()[0].FilePath);
		}
		
		[TestMethod]
		public void QueryFactoryTest_FakeIQuery_IgnoreNoItemsInList()
		{
			var fakeIQuery = new FakeIQuery(new List<FileIndexItem>());
			var query = new QueryFactory(null,fakeIQuery,null,
				null,null, null).Query();
			
			var resultFakeIQuery = query as FakeIQuery;
			Assert.AreEqual(0, resultFakeIQuery?.GetAllRecursive().Count);
		}
	}

}

