using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

		await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
		{
			await sut.ExecuteWithRetryAsync<int>(_ =>
				throw new InvalidOperationException("always"));
		});
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
}
