using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public class QueryFolderTest
	{
		private readonly Query _query;
		private IMemoryCache _memoryCache;

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

		/// <summary>
		/// with cache enabled
		/// </summary>
		public QueryFolderTest()
		{
			_query = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetService<ApplicationDbContext>(),_memoryCache) ;
		}
		
		[TestMethod]
		public void CacheGetParentFolder_FallbackWhenNoCache()
		{
			var queryNoCache = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetService<ApplicationDbContext>()) ;

			var result = queryNoCache.CacheGetParentFolder("/");
			Assert.IsFalse(result.Item1);
		}
		
		[TestMethod]
		public void CacheGetParentFolder_FallbackWhenNoCache_appSettings()
		{
			var queryNoCache = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetService<ApplicationDbContext>(),_memoryCache, new AppSettings{ AddMemoryCache = false}) ;

			var result = queryNoCache.CacheGetParentFolder("/");
			Assert.IsFalse(result.Item1);
		}
	}
}
