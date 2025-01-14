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
public class QueryExistsTests
{
	private readonly IMemoryCache? _memoryCache;

	public QueryExistsTests()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		_memoryCache = provider.GetService<IMemoryCache>();
	}

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(QueryTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task ExistsAsync_Disposed()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext,
			new AppSettings { Verbose = true }, serviceScope, new FakeIWebLogger(),
			_memoryCache);
		const string path = "/ExistsAsync_Disposed/test.jpg";
		await query.AddItemAsync(
			new FileIndexItem(path) { Tags = "hi" });

		// important to Dispose
		await dbContext.DisposeAsync();

		var getItem = await query.ExistsAsync(path);
		Assert.IsTrue(getItem);
	}


	[DataTestMethod]
	[DataRow("/", true, true)]
	[DataRow("/test.jpg", false, true)]
	[DataRow("/test/", true, true)]
	[DataRow("/notfound.jpg", false, false)]
	public async Task ExistsAsync_HomePathAndNotFound(string filePath, bool isDirectory,
		bool expected)
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext,
			new AppSettings { Verbose = true }, serviceScope, new FakeIWebLogger(),
			_memoryCache);

		if ( expected )
		{
			if ( filePath != "/" )
			{
				filePath = PathHelper.RemoveLatestSlash(filePath);
			}

			await query.AddItemAsync(
				new FileIndexItem(filePath) { Tags = "item", IsDirectory = isDirectory });
		}

		var result = await query.ExistsAsync(filePath);
		Assert.AreEqual(expected, result);
	}
}
