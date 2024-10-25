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
		Assert.AreEqual(0, items.Count);
	}

	[TestMethod]
	public async Task GetObjectsByFileHashAsyncTest_GetByHash()
	{
		await _query.AddItemAsync(new FileIndexItem { FileHash = "123456" });
		var items = await _query.GetObjectsByFileHashAsync(new List<string> { "123456" });

		Assert.AreEqual(1, items.Count);
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
		await Assert.ThrowsExceptionAsync<AggregateException>(async () =>
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
		await dbContext.SaveChangesAsync();

		// And dispose
		await dbContext.DisposeAsync();

		// Act
		var result = await query.GetObjectsByFileHashAsync(new List<string> { "test123" }, 1);

		// Assert
		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("test123", result[0].FileHash);
	}
}
