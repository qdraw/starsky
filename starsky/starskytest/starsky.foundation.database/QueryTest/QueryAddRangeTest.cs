using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

// ReSharper disable once IdentifierTypo
namespace starskytest.starsky.foundation.database.QueryTest;

/// <summary>
///     AddRangeAsyncTest
/// </summary>
[TestClass]
[SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeEvident")]
public class QueryAddRangeTest
{
	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(QueryAddRangeTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task AddRangeAsync()
	{
		var expectedResult = new List<FileIndexItem>
		{
			new FileIndexItem { FileHash = "TEST4" }, new FileIndexItem { FileHash = "TEST5" }
		};

		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		var query = new Query(dbContext,
			new AppSettings { AddMemoryCache = false, Verbose = true },
			serviceScopeFactory, new FakeIWebLogger(), new FakeMemoryCache()
		);
		await query.AddRangeAsync(expectedResult);

		var queryFromDb = dbContext.FileIndex
			.Where(p => p.FileHash == "TEST4" || p.FileHash == "TEST5").ToList();
		Assert.AreEqual(expectedResult.FirstOrDefault()?.FileHash,
			queryFromDb.FirstOrDefault()?.FileHash);
		Assert.AreEqual(expectedResult[1].FileHash, queryFromDb[1].FileHash);
	}

	[TestMethod]
	public async Task AddRangeAsync_Disposed()
	{
		var expectedResult = new List<FileIndexItem>
		{
			new FileIndexItem { FileHash = "TEST4" }, new FileIndexItem { FileHash = "TEST5" }
		};

		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContextDisposed = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		// Dispose here
		await dbContextDisposed.DisposeAsync();

		await new Query(dbContextDisposed,
			new AppSettings { AddMemoryCache = false }, serviceScopeFactory, new FakeIWebLogger(),
			new FakeMemoryCache()).AddRangeAsync(expectedResult);

		var context = new InjectServiceScope(serviceScopeFactory).Context();
		var queryFromDb = context.FileIndex
			.Where(p => p.FileHash == "TEST4" || p.FileHash == "TEST5").ToList();

		Assert.AreEqual(expectedResult.FirstOrDefault()?.FileHash,
			queryFromDb.FirstOrDefault()?.FileHash);
		Assert.AreEqual(expectedResult[1].FileHash, queryFromDb[1].FileHash);
	}

	[TestMethod]
	public async Task AddRange()
	{
		var expectedResult = new List<FileIndexItem>
		{
			new FileIndexItem { FileHash = "TEST4" }, new FileIndexItem { FileHash = "TEST5" }
		};

		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		await new Query(dbContext,
			new AppSettings { AddMemoryCache = false }, serviceScopeFactory, new FakeIWebLogger(),
			new FakeMemoryCache()).AddRangeAsync(expectedResult);

		var queryFromDb = dbContext.FileIndex
			.Where(p => p.FileHash == "TEST4" || p.FileHash == "TEST5").ToList();
		Assert.AreEqual(expectedResult.FirstOrDefault()?.FileHash,
			queryFromDb.FirstOrDefault()?.FileHash);
		Assert.AreEqual(expectedResult[1].FileHash, queryFromDb[1].FileHash);
	}

	[TestMethod]
	public async Task AddRangeAsync_DbUpdateConcurrencyException()
	{
		var expectedResult = new List<FileIndexItem>
		{
			new FileIndexItem { FileHash = "TEST4" }, new FileIndexItem { FileHash = "TEST5" }
		};
		var serviceScopeFactory = CreateNewScope();
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;
		var dbContext = new ConcurrencyExceptionApplicationDbContext(options);

		var webLogger = new FakeIWebLogger();
		await new Query(dbContext,
			new AppSettings { AddMemoryCache = false }, serviceScopeFactory, webLogger,
			new FakeMemoryCache()).AddRangeAsync(expectedResult);

		Assert.AreEqual("[AddRangeAsync] save failed after DbUpdateConcurrencyException",
			webLogger.TrackedExceptions[0].Item2);
	}

	private sealed class ConcurrencyExceptionApplicationDbContext : ApplicationDbContext
	{
		public ConcurrencyExceptionApplicationDbContext(DbContextOptions options) : base(options)
		{
		}

		public override DbSet<FileIndexItem> FileIndex
		{
			get => throw new DbUpdateConcurrencyException();
			set
			{
				// do nothing
			}
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			throw new DbUpdateConcurrencyException();
		}
	}
}
