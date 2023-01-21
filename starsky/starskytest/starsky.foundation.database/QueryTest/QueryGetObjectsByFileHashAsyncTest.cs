using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddMemoryCache();
		services.AddDbContext<ApplicationDbContext>(options => 
			options.UseInMemoryDatabase(nameof(GetObjectsByFileHashAsyncTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	public GetObjectsByFileHashAsyncTest()
	{
		_query = new Query(CreateNewScope().CreateScope().ServiceProvider
			.GetService<ApplicationDbContext>(), new AppSettings(), null!, new FakeIWebLogger(), new FakeMemoryCache()) ;
	}
	
	[TestMethod]
	public async Task GetObjectsByFileHashAsyncTest_NoContent()
	{
		var items =  await _query.GetObjectsByFileHashAsync(new List<string>());
		Assert.AreEqual(0, items.Count);
	}
	
	[TestMethod]
	public async Task GetObjectsByFileHashAsyncTest_GetByHash()
	{
		await _query.AddItemAsync(new FileIndexItem(){FileHash = "123456"});
		var items =  await _query.GetObjectsByFileHashAsync(new List<string>
		{
			"123456"
		});
		
		Assert.AreEqual(1, items.Count);
		Assert.AreEqual( "123456", items.FirstOrDefault(p => p.FileHash == "123456")?.FileHash);
	}	
}
