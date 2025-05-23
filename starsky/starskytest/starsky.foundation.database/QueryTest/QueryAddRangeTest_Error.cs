using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public class QueryAddRangeTest_Error
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
		await fakeQuery.AddRangeAsync(new List<FileIndexItem> { new("/test22.jpg") });

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
		await fakeQuery.AddRangeAsync(new List<FileIndexItem> { new("/test22.jpg") });

		Assert.AreEqual(1, dbUpdateExceptionDbContext.Count);
	}
}

internal sealed class SqliteExceptionDbContext(DbContextOptions options)
	: ApplicationDbContext(options)
{
	public int Count { get; set; }


#pragma warning disable 8603
	public override DbSet<FileIndexItem> FileIndex => null;
#pragma warning restore 8603

	public override int SaveChanges()
	{
		Count++;
		if ( Count == 1 )
		{
			throw new SqliteException("t", 1, 2);
		}

		return Count;
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
