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

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public sealed class QueryCountTest
	{
		private IMemoryCache _memoryCache;
		private readonly Query _query;

		public QueryCountTest()
		{
			_query = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetService<ApplicationDbContext>(), new AppSettings(), CreateNewScope(), new FakeIWebLogger(),_memoryCache);
		}

		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddMemoryCache();
			services.AddDbContext<ApplicationDbContext>(options => 
				options.UseInMemoryDatabase(nameof(QueryGetObjectsByFilePathAsyncTest)));
			var serviceProvider = services.BuildServiceProvider();
			_memoryCache = serviceProvider.GetService<IMemoryCache>();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task ShouldGive1Result_BasicQuery()
		{
			// make sure the database is empty
			foreach ( var item in await _query.GetAllRecursiveAsync() )
			{
				await _query.RemoveItemAsync(item);
			}
			
			var itemAsync = await _query.AddItemAsync(new FileIndexItem("/test.jpg"));
			var result = await _query.CountAsync();
			
			Assert.AreEqual(1,result);
			await _query.RemoveItemAsync(itemAsync);
		}
		
		[TestMethod]
		public async Task ShouldGive1Result_Predicate()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => 
				options.UseInMemoryDatabase(nameof(QueryGetObjectsByFilePathAsyncTest) + "ShouldGive1Result_Predicate"));
			var serviceProvider = services.BuildServiceProvider();
			
			var query = new Query(serviceProvider.GetService<ApplicationDbContext>(), new AppSettings(), 
				null!, new FakeIWebLogger(),_memoryCache);
			
			var itemAsync = await query.AddItemAsync(new FileIndexItem("/test.jpg"));
			var result = await query.CountAsync(p => p.IsDirectory == false);
			
			Assert.AreEqual(1,result);
			await query.RemoveItemAsync(itemAsync);
		}
	
	}
}

