
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Data;
using starskycore.Extensions;

namespace starskytest.Extensions
{
	[TestClass]
	public class TestConnectionTest
	{
		[TestMethod]
		public void TestConnection_Default()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			
			var context = new ApplicationDbContext(options);
			Assert.AreEqual(true,context.TestConnection());
		}
	} 
}
		
