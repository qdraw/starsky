using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using starsky.foundation.database.Data;
using starsky.foundation.database.Extensions;
using starskytest.FakeMocks;

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
			Assert.AreEqual(true,context.TestConnection(new FakeIWebLogger()));
		}
		
		[TestMethod]
		public void TestConnection_Mysql_Default()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseMySql("Server=localhost;Port=1234;database=test;uid=test;pwd=test;", 
					ServerVersion.Create(5, 0, 0,ServerType.MariaDb))
				.Options;
			
			var context = new ApplicationDbContext(options);
			Assert.AreEqual(true,context.TestConnection(new FakeIWebLogger()));
		}
	} 
}
