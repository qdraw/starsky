using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Query;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public class QueryFactoryTest
	{
		[TestMethod]
		public void QueryFactoryTest_Null()
		{
			var query = new QueryFactory(null,null,null,null,null).Query();
			Assert.IsNull(query);
		}
		
		[TestMethod]
		public void QueryFactoryTest_QueryReturn()
		{
			var query = new QueryFactory(null,new Query(null,null, null, new FakeIWebLogger()),null,null,null).Query();
			Assert.AreEqual(typeof(Query),query.GetType());
		}
	}

}

