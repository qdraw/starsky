using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Helpers;

[TestClass]
public class ExecuteWithRetryTest
{
	public TestContext TestContext { get; set; }

	private static IServiceScopeFactory CreateNewScope(string name)
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(name));
		var sp = services.BuildServiceProvider();
		return sp.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task ExecuteWithRetry_SucceedsImmediate_NoScopeFactory()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("ExecuteWithRetry_SucceedsImmediate_NoScopeFactory")
			.Options;
		await using var db = new ApplicationDbContext(options);

		var logger = new FakeIWebLogger();
		var sut = new ExecuteWithRetry(db, null, logger);

		var res = await sut.ExecuteWithRetryAsync(_ => Task.FromResult(42));
		Assert.AreEqual(42, res);
	}

	[TestMethod]
	public async Task ExecuteWithRetry_ThrowsObjectDisposed_NoScopeFactory()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("ExecuteWithRetry_ThrowsObjectDisposed_NoScopeFactory")
			.Options;
		var db = new ApplicationDbContext(options);
		await db.DisposeAsync();

		var logger = new FakeIWebLogger();
		var sut = new ExecuteWithRetry(db, null, logger);

		await Assert.ThrowsExactlyAsync<ObjectDisposedException>(async () =>
		{
			await sut.ExecuteWithRetryAsync(async ctx =>
				await ctx.Thumbnails.CountAsync(TestContext.CancellationToken));
		});
	}

	[TestMethod]
	public async Task ExecuteWithRetry_RetriesThenSucceeds_NoScopeFactory()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("ExecuteWithRetry_RetriesThenSucceeds_NoScopeFactory")
			.Options;
		await using var db = new ApplicationDbContext(options);

		var logger = new FakeIWebLogger();
		var sut = new ExecuteWithRetry(db, null, logger);

		var call = 0;
		var res = await sut.ExecuteWithRetryAsync(_ =>
		{
			call++;
			if ( call == 1 )
			{
				throw new InvalidOperationException("transient");
			}

			return Task.FromResult(7);
		});

		Assert.AreEqual(7, res);
		Assert.IsGreaterThanOrEqualTo(2, call);
		Assert.IsNotEmpty(logger.TrackedWarnings);
	}

	[TestMethod]
	public async Task ExecuteWithRetry_AlwaysTransient_Propagates_NoScopeFactory()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("ExecuteWithRetry_AlwaysTransient_Propagates_NoScopeFactory")
			.Options;
		await using var db = new ApplicationDbContext(options);

		var logger = new FakeIWebLogger();
		var sut = new ExecuteWithRetry(db, null, logger);

		var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
		{
			await sut.ExecuteWithRetryAsync<int>(_ =>
				throw new InvalidOperationException("always"));
		});

		Assert.AreEqual("ExecuteWithRetryAsync exhausted retries", ex.Message);
	}

	[TestMethod]
	public async Task ExecuteWithRetry_AlwaysTransient_Propagates_WithScopeFactory()
	{
		var scopeFactory =
			CreateNewScope("ExecuteWithRetry_AlwaysTransient_Propagates_WithScopeFactory");
		using var serviceScope = scopeFactory.CreateScope();
		var db = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		var logger = new FakeIWebLogger();
		var sut = new ExecuteWithRetry(db, scopeFactory, logger);

		var ex = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
		{
			await sut.ExecuteWithRetryAsync<int>(_ =>
				throw new InvalidOperationException("always"));
		});

		Assert.AreEqual("ExecuteWithRetryAsync exhausted retries", ex.Message);
	}

	[TestMethod]
	public async Task ExecuteWithRetry_NonTransient_Propagates_NoScopeFactory()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("ExecuteWithRetry_NonTransient_Propagates_NoScopeFactory")
			.Options;
		await using var db = new ApplicationDbContext(options);

		var logger = new FakeIWebLogger();
		var sut = new ExecuteWithRetry(db, null, logger);

		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
		{
			await sut.ExecuteWithRetryAsync<int>(_ => throw new ArgumentException("bad"));
		});
	}

	[TestMethod]
	public async Task ExecuteWithRetry_Succeeds_WithScopeFactory()
	{
		var scopeFactory = CreateNewScope("ExecuteWithRetry_Succeeds_WithScopeFactory");
		// create a context for injection as well
		using var serviceScope = scopeFactory.CreateScope();
		var db = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		var logger = new FakeIWebLogger();
		var sut = new ExecuteWithRetry(db, scopeFactory, logger);

		var res = await sut.ExecuteWithRetryAsync(_ => Task.FromResult(99));
		Assert.AreEqual(99, res);
	}

	[TestMethod]
	public async Task ExecuteWithRetry_RetriesThenSucceeds_WithScopeFactory()
	{
		var scopeFactory = CreateNewScope("ExecuteWithRetry_RetriesThenSucceeds_WithScopeFactory");
		using var serviceScope = scopeFactory.CreateScope();
		var db = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		var logger = new FakeIWebLogger();
		var sut = new ExecuteWithRetry(db, scopeFactory, logger);

		var call = 0;
		var res = await sut.ExecuteWithRetryAsync(_ =>
		{
			call++;
			return call == 1
				? throw new InvalidOperationException("transient")
				: Task.FromResult(123);
		});

		Assert.AreEqual(123, res);
		Assert.IsGreaterThanOrEqualTo(2, call);
		Assert.IsNotEmpty(logger.TrackedWarnings);
	}

	[TestMethod]
	public void IsTransientDbException_Null_ReturnsFalse()
	{
		Assert.IsFalse(ExecuteWithRetry.IsTransientDbException(null));
	}

	[TestMethod]
	public void IsTransientDbException_InvalidOperationException_ReturnsTrue()
	{
		Assert.IsTrue(ExecuteWithRetry.IsTransientDbException(new InvalidOperationException()));
	}

	[TestMethod]
	public void IsTransientDbException_NullReferenceException_ReturnsTrue()
	{
		Assert.IsTrue(ExecuteWithRetry.IsTransientDbException(new NullReferenceException()));
	}

	[TestMethod]
	public void IsTransientDbException_MySqlExceptionInstance_ReturnsTrue()
	{
		// Create an instance of MySqlException without invoking constructor
		var mysqlType = typeof(MySqlException);
		var mysqlEx = ( Exception ) RuntimeHelpers.GetUninitializedObject(mysqlType);
		Assert.IsTrue(ExecuteWithRetry.IsTransientDbException(mysqlEx));
	}

	[TestMethod]
	public void IsTransientDbException_InnerExceptionChainContainingMySql_ReturnsTrue()
	{
		var mysqlType = typeof(MySqlException);
		var mysqlEx = ( Exception ) RuntimeHelpers.GetUninitializedObject(mysqlType);

		var inner = new Exception("inner", mysqlEx);
		var outer = new Exception("outer", inner);

		Assert.IsTrue(ExecuteWithRetry.IsTransientDbException(outer));
	}

	[TestMethod]
	public void IsTransientDbException_ArgumentException_ReturnsFalse()
	{
		Assert.IsFalse(ExecuteWithRetry.IsTransientDbException(new ArgumentException("bad")));
	}

	[TestMethod]
	public void IsTransientDbException_InnerChainWithoutMySql_ReturnsFalse()
	{
		var inner = new Exception("inner", new InvalidCastException());
		var outer = new Exception("outer", inner);
		Assert.IsFalse(ExecuteWithRetry.IsTransientDbException(outer));
	}
}
