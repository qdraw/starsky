using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using starsky.foundation.database.Data;
using starsky.foundation.database.Extensions;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Extensions;

[TestClass]
public sealed class EntityFrameworkExtensionsTest
{
	[TestMethod]
	public void TestConnection_Default()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;

		var context = new ApplicationDbContext(options);
		Assert.IsTrue(context.TestConnection(new FakeIWebLogger()));
	}

	[TestMethod]
	public void TestConnection_Mysql_Default()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseMySql("Server=localhost;Port=1234;database=test;uid=test;pwd=test;",
				ServerVersion.Create(5, 0, 0, ServerType.MariaDb))
			.Options;

		var context = new ApplicationDbContext(options);
		Assert.IsTrue(context.TestConnection(new FakeIWebLogger()));
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
		var memoryCache = provider.GetRequiredService<IMemoryCache>();

		var result = context.TestConnection(new FakeIWebLogger(), memoryCache);
		Assert.IsTrue(result);

		memoryCache.TryGetValue("TestConnection", out bool? result2);
		Assert.IsTrue(result2);
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
		var memoryCache = provider.GetRequiredService<IMemoryCache>();
		memoryCache.Set("TestConnection", false);

		var result = context.TestConnection(new FakeIWebLogger(), memoryCache);
		Assert.IsFalse(result);

		memoryCache.TryGetValue("TestConnection", out bool? result2);
		Assert.IsFalse(result2);
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

		Assert.IsFalse(result);
		Assert.IsTrue(logger.TrackedInformation.FirstOrDefault().Item2
			?.Contains("Database is not available"));
	}


	private sealed class AppDbMySqlException : ApplicationDbContext
	{
		public AppDbMySqlException(DbContextOptions options) : base(options)
		{
		}

		public override DatabaseFacade Database =>
			throw CreateMySqlException("Database is not available");

		private static MySqlException CreateMySqlException(string message)
		{
			// MySqlErrorCode errorCode, string? sqlState, string message, Exception? innerException

			var ctorLIst =
				typeof(MySqlException).GetConstructors(
					BindingFlags.Instance |
					BindingFlags.NonPublic | BindingFlags.InvokeMethod);
			var ctor = ctorLIst.FirstOrDefault(p =>
				p.ToString() ==
				"Void .ctor(MySqlConnector.MySqlErrorCode, System.String, System.String, System.Exception)");

			var instance =
				( MySqlException? ) ctor?.Invoke(new object[]
				{
					MySqlErrorCode.AccessDenied, "test", message, new Exception()
				});
			return instance!;
		}
	}
}
