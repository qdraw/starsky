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
	public sealed class QueryFolderTest
	{
		private IMemoryCache? _memoryCache;

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
				CreateNewScope(), new FakeIWebLogger(),_memoryCache) ;

			var result = queryNoCache.CacheGetParentFolder("/");
			Assert.IsFalse(result.Item1);
		}
		
		[TestMethod]
		public void CacheGetParentFolder_FallbackWhenNoCache_appSettings()
		{
			var queryNoCache = new Query(CreateNewScope().CreateScope().ServiceProvider
					.GetRequiredService<ApplicationDbContext>(), 
				new AppSettings{ AddMemoryCache = false}, null!, new FakeIWebLogger(),_memoryCache) ;

			var result = queryNoCache.CacheGetParentFolder("/");
			Assert.IsFalse(result.Item1);
		}
		
		[TestMethod]
		public async Task GetNextPrevInFolder_Next_DisposedItem()
		{
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			// remove all items
			foreach ( var allSingleItem in await dbContext.FileIndex.ToListAsync() )
			{
				dbContext.FileIndex.Remove(allSingleItem);
			}
			await dbContext.SaveChangesAsync();
			// and remove all folders
			
			// item sub folder
			var item = new FileIndexItem("/test_1234567832/test_0191921.jpg");
			dbContext.FileIndex.Add(item);
			
			var item1 = new FileIndexItem("/test_1234567832/test_0191922.jpg");
			dbContext.FileIndex.Add(item1);
			
			await dbContext.SaveChangesAsync();
	        
			// Important to dispose!
			await dbContext.DisposeAsync();
			
			var query = new Query(dbContext, 
				new AppSettings(), serviceScope, new FakeIWebLogger(),_memoryCache);
			
			var getItem = query.GetNextPrevInFolder("/test_1234567832/test_0191921.jpg");
			Assert.IsNotNull(getItem);
			Assert.AreEqual("/test_1234567832/test_0191922.jpg", getItem.NextFilePath);

			await query.RemoveItemAsync(item);
			await query.RemoveItemAsync(item1);
		}
	}
}
