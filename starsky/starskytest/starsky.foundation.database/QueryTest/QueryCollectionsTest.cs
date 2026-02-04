using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public class QueryCollectionsTest
{
	[TestMethod]
	public void QueryCollections_StackCollections_1()
	{
		var input = new List<FileIndexItem> { new("/test.jpg"), new("/test.dng") };
		var result = Query.StackCollections(input);

		Assert.HasCount(1, result);
		Assert.AreEqual("/test.jpg", result[0].FilePath);
	}

	[TestMethod]
	public void QueryCollections_StackCollections_2()
	{
		var input = new List<FileIndexItem> { new("/test.jpg"), new("/test.mp4") };
		var result = Query.StackCollections(input);

		Assert.HasCount(1, result);
		Assert.AreEqual("/test.jpg", result[0].FilePath);
	}
}
