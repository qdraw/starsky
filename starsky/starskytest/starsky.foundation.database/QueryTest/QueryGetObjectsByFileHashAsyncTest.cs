using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public class GetObjectsByFileHashAsyncTest
{
	private readonly Query _query;

	public GetObjectsByFileHashAsyncTest()
	{
		_query = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetRequiredService<ApplicationDbContext>(), new AppSettings(), null!,
			new FakeIWebLogger(), new FakeMemoryCache());
	}

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddMemoryCache();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(GetObjectsByFileHashAsyncTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task GetObjectsByFileHashAsyncTest_NoContent()
	{
		var items = await _query.GetObjectsByFileHashAsync(new List<string>());
		Assert.IsEmpty(items);
	}

	[TestMethod]
	public async Task GetObjectsByFileHashAsyncTest_GetByHash()
	{
		await _query.AddItemAsync(new FileIndexItem { FileHash = "123456" });
		var items = await _query.GetObjectsByFileHashAsync(new List<string> { "123456" });

		Assert.HasCount(1, items);
		Assert.AreEqual("123456", items.Find(p => p.FileHash == "123456")?.FileHash);
	}

	[TestMethod]
	public async Task GetObjectsByFileHashAsync_Disposed_NoServiceScope()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("Add_writes_to_database11")
			.Options;

		var dbContext = new ApplicationDbContext(options);
		var query =
			new Query(dbContext, new AppSettings(), null!,
				new FakeIWebLogger()); // <-- no service scope

		// Dispose the DbContext
		await dbContext.DisposeAsync();

		// Act & Assert
		await Assert.ThrowsExactlyAsync<AggregateException>(async () =>
			await query.GetObjectsByFileHashAsync(new List<string> { "test123" }, 1));
	}

	[TestMethod]
	public async Task GetObjectsByFileHashAsync_Disposed_Success()
	{
		// Arrange
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider
			.GetRequiredService<ApplicationDbContext>();
		var query =
			new Query(dbContext, new AppSettings(), serviceScope,
				new FakeIWebLogger()); // <-- no service scope

		dbContext.FileIndex.Add(new FileIndexItem { FileHash = "test123" });
		await dbContext.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		// And dispose
		await dbContext.DisposeAsync();

		// Act
		var result = await query.GetObjectsByFileHashAsync(new List<string> { "test123" }, 1);

		// Assert
		Assert.HasCount(1, result);
		Assert.AreEqual("test123", result[0].FileHash);
	}

	public TestContext TestContext { get; set; }
}

[TestClass]
public sealed class QueryGetItemsByHash_ObjectDisposedException_Test
{
	[TestMethod]
	public async Task GetSubPathsByHashAsync_WhenPrimaryContextThrows_ObjectDisposed_UsesScopedContext()
	{
		// Arrange: primary context that throws ObjectDisposedException when FileIndex accessed
		var primaryOptions = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
		var primary = new ThrowingObjectDisposedDbContext(primaryOptions);

		// Create a fake scope factory that returns a real in-memory context
		var scopeFactory = new FakeIServiceScopeFactory(nameof(QueryGetItemsByHash_ObjectDisposedException_Test));
		var scope = scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		var expected = new FileIndexItem("/obj/file.jpg") { FileName = "file.jpg", ParentDirectory = "/obj", FileHash = "hash_obj" };
		dbContext.FileIndex.Add(expected);
		await dbContext.SaveChangesAsync(TestContext.CancellationToken);

		var query = new Query(primary, new AppSettings { AddMemoryCache = false }, scopeFactory, new FakeIWebLogger());

		// Act
		var result = await query.GetSubPathsByHashAsync(expected.FileHash!);

		// Assert
		Assert.IsNotNull(result);
		Assert.Contains(expected.FilePath, result);
	}

	private sealed class ThrowingObjectDisposedDbContext : ApplicationDbContext
	{
		public ThrowingObjectDisposedDbContext(DbContextOptions options) : base(options) { }

		public override DbSet<FileIndexItem> FileIndex
		{
			get => throw new ObjectDisposedException("Simulated disposed context");
			set
			{
				// do nothing here
			}
		}
	}

	public TestContext TestContext { get; set; }
}
