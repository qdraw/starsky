using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public sealed class QueryFolderTest
{
	private IMemoryCache? _memoryCache;

	public TestContext TestContext { get; set; }

	private IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddMemoryCache();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(QueryGetObjectsByFilePathAsyncTest)));
		var serviceProvider = services.BuildServiceProvider();
		_memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public void CacheGetParentFolder_FallbackWhenNoCache()
	{
		var queryNoCache = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetRequiredService<ApplicationDbContext>(), new AppSettings(),
			CreateNewScope(), new FakeIWebLogger(), _memoryCache);

		var result = queryNoCache.CacheGetParentFolder("/");
		Assert.IsFalse(result.Item1);
	}

	[TestMethod]
	public void CacheGetParentFolder_FallbackWhenNoCache_appSettings()
	{
		var queryNoCache = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetRequiredService<ApplicationDbContext>(),
			new AppSettings { AddMemoryCache = false }, null!, new FakeIWebLogger(), _memoryCache);

		var result = queryNoCache.CacheGetParentFolder("/");
		Assert.IsFalse(result.Item1);
	}

	[TestMethod]
	public void CacheGetParentFolder_ShouldReturnCachedItems_Duplicates()
	{
		var query = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetRequiredService<ApplicationDbContext>(), new AppSettings(),
			CreateNewScope(), new FakeIWebLogger(), _memoryCache);

		// Arrange
		const string subPath = "/test_folder";
		var fileIndexItems = new List<FileIndexItem>
		{
			new() { FilePath = "/test_folder/file1.jpg" },
			new() { FilePath = "/test_folder/file2.jpg" },
			new() { FilePath = "/test_folder/file2.jpg" }
		};
		var cacheKey = Query.CachingDbName(nameof(FileIndexItem), subPath);
		_memoryCache?.Set(cacheKey, fileIndexItems);

		// Act
		var result = query.CacheGetParentFolder(subPath);

		// Assert
		Assert.IsTrue(result.Item1);
		Assert.HasCount(2, result.Item2);
		Assert.AreEqual("/test_folder/file1.jpg", result.Item2[0].FilePath);
		Assert.AreEqual("/test_folder/file2.jpg", result.Item2[1].FilePath);
	}

	[TestMethod]
	public async Task GetNextPrevInFolder_Next_DisposedItem()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		// remove all items
		foreach ( var allSingleItem in await dbContext.FileIndex.ToListAsync(TestContext
			         .CancellationTokenSource.Token) )
		{
			dbContext.FileIndex.Remove(allSingleItem);
		}

		await dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
		// and remove all folders

		// item sub folder
		var item = new FileIndexItem("/test_1234567832/test_0191921.jpg");
		dbContext.FileIndex.Add(item);

		var item1 = new FileIndexItem("/test_1234567832/test_0191922.jpg");
		dbContext.FileIndex.Add(item1);

		await dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		// Important to dispose!
		await dbContext.DisposeAsync();

		var query = new Query(dbContext,
			new AppSettings(), serviceScope, new FakeIWebLogger(), _memoryCache);

		var getItem = query.GetNextPrevInFolder("/test_1234567832/test_0191921.jpg");
		Assert.IsNotNull(getItem);
		Assert.AreEqual("/test_1234567832/test_0191922.jpg", getItem.NextFilePath);

		await query.RemoveItemAsync(item);
		await query.RemoveItemAsync(item1);
	}

	[TestMethod]
	public void DisplayFileFolders_AppliesStructuredFilters()
	{
		var query = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetRequiredService<ApplicationDbContext>(),
			new AppSettings(),
			CreateNewScope(),
			new FakeIWebLogger(),
			_memoryCache);

		var input = new List<FileIndexItem>
		{
			new()
			{
				FileName = "a.jpg",
				ParentDirectory = "/",
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
				MakeModel = "Canon EOS R5",
				Tags = "holiday, beach",
				DateTime = new DateTime(2026, 4, 10)
			},
			new()
			{
				FileName = "a.arw",
				ParentDirectory = "/",
				ImageFormat = ExtensionRolesHelper.ImageFormat.arw,
				MakeModel = "Sony A7",
				Tags = "city",
				DateTime = new DateTime(2026, 4, 15)
			}
		};

		var result = query.DisplayFileFolders(input,
			null,
			true,
			true,
			"jpg",
			"canon",
			"holiday",
			"2026-04-01",
			"2026-04-30").ToList();

		Assert.HasCount(1, result);
		Assert.AreEqual("a.jpg", result[0].FileName);
	}
}

[TestClass]
public class QueryDisplayFileFolders_NotSupportedException_Test
{
	[TestMethod]
	public void
		QueryDisplayFileFolders_uses_scope_when_primary_context_throws_NotSupportedException()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;

		// Primary context that throws when FileIndex is accessed
		var primary = new ThrowingFileIndexDbContext(options);

		// Create an in-memory context to be returned by the scope
		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
		var scopeContext = new ApplicationDbContext(builder.Options);
		var expectedItem =
			new FileIndexItem("/file.jpg") { FileName = "file.jpg", ParentDirectory = "/" };
		scopeContext.FileIndex.Add(expectedItem);
		scopeContext.SaveChanges();

		// Fake IServiceScopeFactory -> IServiceScope -> IServiceProvider
		var fakeScopeFactory = new FakeServiceScopeFactory(scopeContext);

		var appSettings = new AppSettings();
		var fakeLogger = new FakeIWebLogger();

		var query = new Query(primary, appSettings, fakeScopeFactory, fakeLogger);

		// Act
		var result = query.QueryDisplayFileFolders();

		// Assert - result should come from the scopeContext (the in-memory DB)
		Assert.IsNotNull(result);
		Assert.Contains(r => r.FilePath == expectedItem.FilePath, result);
		Assert.AreEqual(1, fakeScopeFactory.CreateCount);
	}

	// Primary context that throws when FileIndex is accessed
	internal sealed class ThrowingFileIndexDbContext(DbContextOptions options)
		: ApplicationDbContext(options)
	{
		[SuppressMessage("Usage", "S3237:S3237",
			Justification = "Is checked")]
		public override DbSet<FileIndexItem> FileIndex
		{
			get => throw new NotSupportedException("Simulated EF Core read conflict");
			set
			{
				// do nothing
				/* no-op */
			}
		}
	}

	// // Minimal fake service provider/scope factory to return our in-memory context
	private sealed class FakeServiceProvider(ApplicationDbContext ctx) : IServiceProvider
	{
		public object? GetService(Type serviceType)
		{
			return serviceType == typeof(ApplicationDbContext) ? ctx : null;
		}
	}

	private sealed class FakeServiceScopeFactory(ApplicationDbContext ctx) : IServiceScopeFactory
	{
		public int CreateCount { get; private set; }

		public IServiceScope CreateScope()
		{
			CreateCount++;
			return new FakeServiceScope(new FakeServiceProvider(ctx));
		}
	}
}
