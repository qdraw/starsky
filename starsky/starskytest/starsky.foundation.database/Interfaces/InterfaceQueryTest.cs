using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Interfaces;

[TestClass]
public class InterfaceQueryTest
{
	[TestMethod]
	public void InterfaceQueryTest_SingleItem()
	{
		var query = new FakeIQuery() as IQuery;
		
		var result = query.SingleItem("/", new List<ColorClassParser.Color>());
		
		Assert.IsNull(result);
	}
	
	[TestMethod]
	[ExpectedException(typeof(NotImplementedException))]
	public void InterfaceQueryTest_SingleItem2()
	{
		var query = new FakeIQuery() as IQuery;
		
		query.SingleItem(new List<FileIndexItem>(),"/", 
			new List<ColorClassParser.Color>());
		// implement this
	}
}
