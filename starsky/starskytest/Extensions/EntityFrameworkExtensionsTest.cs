using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using starsky.foundation.database.Data;
using starsky.foundation.database.Extensions;
using starsky.foundation.database.Models;
using starskytest.FakeMocks;

namespace starskytest.Extensions
{
	[TestClass]
	public sealed class TestConnectionTest
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
		
		[TestMethod]
		public void TestConnection_Cache_ShouldSetAfterWards()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			
			var context = new ApplicationDbContext(options);
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
			
			var result = context.TestConnection(new FakeIWebLogger(), memoryCache);
			Assert.AreEqual(true,result);
			
			memoryCache.TryGetValue("TestConnection", out var result2);
			Assert.AreEqual(true, result2);
		}
		
		[TestMethod]
		public void TestConnection_Cache_ShouldGetBefore()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			
			var context = new ApplicationDbContext(options);
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
			memoryCache.Set("TestConnection", false);
			
			var result = context.TestConnection(new FakeIWebLogger(), memoryCache);
			Assert.AreEqual(false,result);
			
			memoryCache.TryGetValue("TestConnection", out var result2);
			Assert.AreEqual(false, result2);
		}
		
		private static MySqlException CreateMySqlException(string message)
		{
			var info = new SerializationInfo(typeof(Exception),
				new FormatterConverter());
			info.AddValue("Number", 1);
			info.AddValue("SqlState", "SqlState");
			info.AddValue("Message", message);
			info.AddValue("InnerException", new Exception());
			info.AddValue("HelpURL", "");
			info.AddValue("StackTraceString", "");
			info.AddValue("RemoteStackTraceString", "");
			info.AddValue("RemoteStackIndex", 1);
			info.AddValue("HResult", 1);
			info.AddValue("Source", "");
			info.AddValue("WatsonBuckets",  Array.Empty<byte>() );
					
			// private MySqlException(SerializationInfo info, StreamingContext context)
			var ctor =
				typeof(MySqlException).GetConstructors(BindingFlags.Instance |
					BindingFlags.NonPublic | BindingFlags.InvokeMethod).FirstOrDefault();
			var instance =
				( MySqlException ) ctor?.Invoke(new object[]
				{
					info,
					new StreamingContext(StreamingContextStates.All)
				});
			return instance;
		}
		
		private class AppDbMySqlException : ApplicationDbContext
		{
			public AppDbMySqlException(DbContextOptions options) : base(options)
			{
			}
			public override DatabaseFacade Database => throw CreateMySqlException("Database is not available");
		}
		
				
		[TestMethod]
		public void TestConnection_MySqlException()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			
			var context = new AppDbMySqlException(options);
			var logger = new FakeIWebLogger();

			var result = context.TestConnection(logger);
			
			Assert.AreEqual(false,result);
			Assert.IsTrue(logger.TrackedInformation.FirstOrDefault().Item2.Contains("Database is not available"));
		}
	} 
}
