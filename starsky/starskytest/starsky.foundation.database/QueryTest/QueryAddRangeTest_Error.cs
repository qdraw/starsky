using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public class QueryAddRangeTestError
{
	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(QueryTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task AddRangeAsync_SQLiteException()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var scope = CreateNewScope();

		var sqLiteFailContext = new SqliteExceptionDbContext(options);
		Assert.AreEqual(0, sqLiteFailContext.Count);

		var fakeQuery =
			new Query(sqLiteFailContext, new AppSettings(), scope, new FakeIWebLogger());
		await fakeQuery.AddRangeAsync([new("/test22.jpg")]);

		Assert.AreEqual(1, sqLiteFailContext.Count);
	}

	[TestMethod]
	public async Task AddRangeAsync_DbUpdateException()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var scope = CreateNewScope();

		var dbUpdateExceptionDbContext = new DbUpdateExceptionDbContext(options);
		Assert.AreEqual(0, dbUpdateExceptionDbContext.Count);

		var fakeQuery = new Query(dbUpdateExceptionDbContext, new AppSettings(), scope,
			new FakeIWebLogger());
		await fakeQuery.AddRangeAsync([new FileIndexItem("/test22.jpg") { Id = 30 }]);

		Assert.AreEqual(1, dbUpdateExceptionDbContext.Count);
	}

	[TestMethod]
	public async Task AddRangeAsync_UniqueConstraint_UsesDuplicateFilterOnRetry()
	{
		var dbName = $"QueryAddRange_Unique_{Guid.NewGuid()}";
		var dbRoot = new InMemoryDatabaseRoot();

		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(dbName, dbRoot));
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(dbName, dbRoot)
			.Options;

		await using ( var seedContext = new ApplicationDbContext(options) )
		{
			await seedContext.FileIndex.AddAsync(new FileIndexItem("/existing.jpg"), TestContext.CancellationToken);
			await seedContext.SaveChangesAsync(TestContext.CancellationToken);
		}

		await using var failingContext = new SqliteUniqueOnceDbContext(options);
		var query = new Query(failingContext, new AppSettings(), scopeFactory,
			new FakeIWebLogger());

		await query.AddRangeAsync(
		[
			new FileIndexItem("/existing.jpg"),
			new FileIndexItem("/new.jpg")
		]);

		await using var assertContext = new ApplicationDbContext(options);
		var allItems = await assertContext.FileIndex.ToListAsync(TestContext.CancellationToken);

		Assert.ContainsSingle(p => p.FilePath == "/existing.jpg", allItems);
		Assert.ContainsSingle(p => p.FilePath == "/new.jpg", allItems);
	}

	public TestContext TestContext { get; set; }
}

internal sealed class SqliteExceptionDbContext(DbContextOptions options)
	: ApplicationDbContext(options)
{
	public int Count { get; set; }


	public override DbSet<FileIndexItem> FileIndex
	{
		get
		{
			Count++;
#pragma warning disable CS8603 // Possible null reference return.
			return Count == 1 ? throw new SqliteException("t", 1, 2) : null;
#pragma warning restore CS8603 // Possible null reference return.
		}
	}

	public override int SaveChanges()
	{
		Count++;
		return Count == 1 ? throw new SqliteException("t", 1, 2) : Count;
	}

	public override Task<int> SaveChangesAsync(
		CancellationToken cancellationToken = default)
	{
		Count++;
		if ( Count == 1 )
		{
			throw new SqliteException("t", 1, 2);
		}

		return Task.FromResult(Count);
	}
}

internal sealed class DbUpdateExceptionDbContext(DbContextOptions options)
	: ApplicationDbContext(options)
{
	public int Count { get; set; }

	public override Task<int> SaveChangesAsync(
		CancellationToken cancellationToken = default)
	{
		Count++;
		if ( Count == 1 )
		{
			throw new DbUpdateException("t",
				new List<EntityEntry>());
		}

		return Task.FromResult(Count);
	}
}

internal sealed class SqliteUniqueOnceDbContext(DbContextOptions options)
	: ApplicationDbContext(options)
{
	private bool _hasThrown;

	public override async Task<int> SaveChangesAsync(
		CancellationToken cancellationToken = default)
	{
		if ( !_hasThrown )
		{
			_hasThrown = true;
			throw new SqliteException("UNIQUE constraint failed: FileIndex.Id", 19, 19);
		}

		return await base.SaveChangesAsync(cancellationToken);
	}
}
