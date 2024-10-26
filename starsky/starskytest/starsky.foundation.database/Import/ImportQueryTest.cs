using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Import;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.Import;

[TestClass]
public sealed class ImportQueryTest
{
	private readonly ApplicationDbContext _dbContext;
	private readonly ImportQuery _importQuery;
	private readonly IServiceScopeFactory _serviceScope;

	public ImportQueryTest()
	{
		_serviceScope = CreateNewScope();
		var scope = _serviceScope.CreateScope();
		_dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		_importQuery = new ImportQuery(_serviceScope, new FakeConsoleWrapper(),
			new FakeIWebLogger());
	}

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(ImportQueryTest)));
		services.AddSingleton<IConsole, FakeConsoleWrapper>();
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public void TestConnection_True()
	{
		var result = _importQuery.TestConnection();
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void TestConnection_Null()
	{
		var result = new ImportQuery(null, new FakeConsoleWrapper(), new FakeIWebLogger())
			.TestConnection();
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task IsHashInImportDbAsync_True()
	{
		var dbContext = new InjectServiceScope(_serviceScope).Context();

		await dbContext.ImportIndex.AddAsync(new ImportIndexItem
		{
			Status = ImportStatus.Ok, FileHash = "TEST2", AddToDatabase = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync();

		var result = await _importQuery.IsHashInImportDbAsync("TEST2");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task IsHashInImportDbAsync_NotFound()
	{
		_dbContext.ImportIndex.Add(new ImportIndexItem
		{
			Status = ImportStatus.Ok, FileHash = "TEST", AddToDatabase = DateTime.UtcNow
		});

		var result = await _importQuery.IsHashInImportDbAsync("Not-found");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task IsHashInImportDbAsync_ContextFail()
	{
		var result = await new ImportQuery(null,
			new FakeConsoleWrapper(), new FakeIWebLogger()).IsHashInImportDbAsync("TEST");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task AddAsync()
	{
		var expectedResult = new ImportIndexItem { FileHash = "TEST3" };
		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		await new ImportQuery(serviceScopeFactory, new FakeConsoleWrapper(),
			new FakeIWebLogger()).AddAsync(expectedResult);

		var queryFromDb = await dbContext.ImportIndex.FirstOrDefaultAsync(
			p => p.FileHash == expectedResult.FileHash);

		Assert.AreEqual(expectedResult.FileHash, queryFromDb?.FileHash);
	}

	[TestMethod]
	public async Task History()
	{
		var expectedResult = new ImportIndexItem
		{
			AddToDatabase = DateTime.UtcNow, FileHash = "TEST8"
		};
		var serviceScopeFactory = CreateNewScope();

		await new ImportQuery(serviceScopeFactory, new FakeConsoleWrapper(),
			new FakeIWebLogger()).AddAsync(expectedResult);

		var historyResult = new ImportQuery(serviceScopeFactory,
			new FakeConsoleWrapper(), new FakeIWebLogger()).History();

		if ( historyResult.Count == 0 )
		{
			throw new WebException("should not be 0");
		}

		Assert.IsTrue(historyResult.Exists(p => p.FileHash == "TEST8"));
	}

	[TestMethod]
	public async Task AddRangeAsync()
	{
		var expectedResult = new List<ImportIndexItem>
		{
			new() { FileHash = "TEST4" }, new() { FileHash = "TEST5" }
		};
		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		await new ImportQuery(serviceScopeFactory, new FakeConsoleWrapper(),
			new FakeIWebLogger()).AddRangeAsync(expectedResult);

		var queryFromDb = await dbContext.ImportIndex
			.Where(p => p.FileHash == "TEST4" || p.FileHash == "TEST5").ToListAsync();
		Assert.AreEqual(expectedResult.FirstOrDefault()?.FileHash,
			queryFromDb.FirstOrDefault()?.FileHash);
		Assert.AreEqual(expectedResult[1].FileHash, queryFromDb[1].FileHash);
	}

	[TestMethod]
	public void AddRange()
	{
		var expectedResult = new List<ImportIndexItem>
		{
			new() { FileHash = "TEST4" }, new() { FileHash = "TEST5" }
		};
		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		new ImportQuery(serviceScopeFactory, new FakeConsoleWrapper(), new FakeIWebLogger())
			.AddRange(expectedResult);

		var queryFromDb = dbContext
			.ImportIndex.Where(p => p.FileHash == "TEST4" || p.FileHash == "TEST5").ToList();
		Assert.AreEqual(expectedResult.FirstOrDefault()!.FileHash,
			queryFromDb.FirstOrDefault()!.FileHash);
		Assert.AreEqual(expectedResult[1].FileHash, queryFromDb[1].FileHash);
	}

	[TestMethod]
	public async Task RemoveItemAsync_RemovesItemFromImportIndex()
	{
		// Arrange
		var importIndexItem = new ImportIndexItem { Id = 1825 };

		// Set up a temporary in-memory database for testing
		var serviceProvider = new ServiceCollection()
			.AddDbContext<ApplicationDbContext>(options =>
				options.UseInMemoryDatabase("TestDatabase123456789"))
			.BuildServiceProvider();

		using ( var scope = serviceProvider.CreateScope() )
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			// Add the importIndexItem to the import index
			dbContext.ImportIndex.Add(importIndexItem);
			await dbContext.SaveChangesAsync();
		}

		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var console = new FakeConsoleWrapper();
		var logger = new FakeIWebLogger();
		using ( var scope = serviceProvider.CreateScope() )
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var importQuery = new ImportQuery(scopeFactory, console, logger, dbContext);

			// Act
			await importQuery.RemoveItemAsync(importIndexItem);
		}

		// Assert
		using ( var scope = serviceProvider.CreateScope() )
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			// Ensure that the item is removed from the import index
			Assert.IsFalse(await dbContext.ImportIndex.AnyAsync(x => x.Id == importIndexItem.Id));
		}
	}

	[TestMethod]
	public async Task ImportQuery_RemoveAsync_Disposed()
	{
		var addedItems = new List<ImportIndexItem>
		{
			new() { FileHash = "RemoveAsync_Disposed__1" },
			new() { FileHash = "RemoveAsync_Disposed__2" }
		};

		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContextDisposed = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		await dbContextDisposed.ImportIndex.AddRangeAsync(addedItems);
		await dbContextDisposed.SaveChangesAsync();

		// Dispose here
		await dbContextDisposed.DisposeAsync();

		var importQuery = new ImportQuery(serviceScopeFactory, new FakeConsoleWrapper(),
			new FakeIWebLogger(), dbContextDisposed);

		await importQuery.RemoveItemAsync(addedItems[0]);
		await importQuery.RemoveItemAsync(addedItems[1]);

		var context = new InjectServiceScope(serviceScopeFactory).Context();
		var queryFromDb = await context.FileIndex.Where(p =>
			p.FileHash == addedItems[0].FilePath || p.FileHash == addedItems[1].FilePath
		).ToListAsync();

		Assert.AreEqual(0, queryFromDb.Count);
	}

	[TestMethod]
	public async Task RemoveItemAsync_DbUpdateConcurrencyException()
	{
		var addedItems = new List<ImportIndexItem>
		{
			new() { FileHash = "RemoveAsync_Disposed__1" }
		};

		var serviceScopeFactory = CreateNewScope();
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;
		var dbContext = new ConcurrencyExceptionApplicationDbContext(options);

		var webLogger = new FakeIWebLogger();
		var importQuery = new ImportQuery(serviceScopeFactory, new FakeConsoleWrapper(),
			webLogger, dbContext);

		await importQuery.RemoveItemAsync(addedItems[0], 1);

		Assert.IsTrue(webLogger.TrackedInformation[0].Item2?.StartsWith(
			"Import [RemoveItemAsync] catch-ed " +
			"DbUpdateConcurrencyException (retry)"));
		Assert.IsTrue(webLogger.TrackedInformation[1].Item2?
			.StartsWith("Import [RemoveItemAsync] catch-ed " +
			            "AggregateException (ignored after retry)"));
	}

	private class ConcurrencyExceptionApplicationDbContext : ApplicationDbContext
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
