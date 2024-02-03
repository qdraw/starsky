using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public sealed class QueryCountTest
	{
		
		[TestMethod]
		public async Task ShouldGive1Result_BasicQuery()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => 
				options.UseInMemoryDatabase(nameof(ShouldGive1Result_BasicQuery)));

			var query =
				new Query(
					services.BuildServiceProvider()
						.GetRequiredService<ApplicationDbContext>(), new AppSettings(),
					null!, new FakeIWebLogger());
			
			
			var itemAsync = await query.AddItemAsync(new FileIndexItem("/test.jpg"));
			var result = await query.CountAsync();
			
			Assert.AreEqual(1,result);
			await query.RemoveItemAsync(itemAsync);
		}
		
		[TestMethod]
		public async Task ShouldGive1Result_Predicate()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => 
				options.UseInMemoryDatabase(nameof(QueryGetObjectsByFilePathAsyncTest) + "ShouldGive1Result_Predicate"));
			var serviceProvider = services.BuildServiceProvider();
			
			var query = new Query(serviceProvider.GetRequiredService<ApplicationDbContext>(), new AppSettings(), 
				null!, new FakeIWebLogger());
			
			var itemAsync = await query.AddItemAsync(new FileIndexItem("/test.jpg"));
			var result = await query.CountAsync(p => p.IsDirectory == false);
			
			Assert.AreEqual(1,result);
			await query.RemoveItemAsync(itemAsync);
		}
	
	}
}

