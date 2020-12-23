using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public class QueryGetAllRecursiveTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly IQuery _query;
				
		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(QueryGetAllFilesTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		public QueryGetAllRecursiveTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			_query = new Query(dbContext,_memoryCache, 
				new AppSettings{Verbose = true}, serviceScope);
		}
		
		[TestMethod]
		public async Task ShouldGiveMultipleResultsBack()
		{
			await _query.AddItemAsync(
				new FileIndexItem("/recursive_test/image0.jpg"));
			await _query.AddItemAsync(
				new FileIndexItem("/recursive_test/sub/image1.jpg"));
			await _query.AddItemAsync(
				new FileIndexItem("/recursive_test/image2.jpg"));
			await _query.AddItemAsync(
				new FileIndexItem("/recursive_test/image3.jpg"));

			
			var result = await _query.GetAllRecursiveAsync(new List<string>
			{
				"/recursive_test/"
			});
			
			Assert.AreEqual("/recursive_test/image0.jpg", result[0].FilePath);
			Assert.AreEqual("/recursive_test/image2.jpg", result[1].FilePath);
			Assert.AreEqual("/recursive_test/image3.jpg", result[2].FilePath);
			Assert.AreEqual("/recursive_test/sub/image1.jpg", result[3].FilePath);

			await _query.RemoveItemAsync(result[0]);
			await _query.RemoveItemAsync(result[1]);
			await _query.RemoveItemAsync(result[2]);
			await _query.RemoveItemAsync(result[3]);
		}
	}
}
