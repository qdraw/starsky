using System.Linq;
using System.Threading.Tasks;
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
	public class Query_AddParentItemsAsyncTest
	{
		private readonly IMemoryCache _memoryCache;
		private Query _query;
		private ApplicationDbContext _dbContext;

		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(Query_AddParentItemsAsyncTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		public Query_AddParentItemsAsyncTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			_dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			_query = new Query(_dbContext,_memoryCache, new AppSettings(), serviceScope);
		}

		[TestMethod]
		public async Task CheckIfHomeItemIsCreated()
		{
			await _query.AddParentItemsAsync("/");
			var result = await _dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/");
			Assert.AreEqual("/", result.FilePath);
			
			_dbContext.FileIndex.Remove(result);
			await _dbContext.SaveChangesAsync();
		}
		
		[TestMethod]
		public async Task Check_If_Slash_Is_Created()
		{
			await _query.AddParentItemsAsync("/test");
			var result = await _dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/");
			Assert.AreEqual("/", result.FilePath);
			
			_dbContext.FileIndex.Remove(result);
			await _dbContext.SaveChangesAsync();
		}
		
		[TestMethod]
		public async Task Check_If_Parent_Is_Created()
		{
			await _query.AddParentItemsAsync("/test/file");
			var result = await _dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test");
			Assert.AreEqual("/test", result.FilePath);
			
			_dbContext.FileIndex.Remove(result);
			await _dbContext.SaveChangesAsync();
		}

		[TestMethod]
		public async Task MissingFolderIsAdded()
		{
			await _query.AddParentItemsAsync("/test/test/test");
			
			var folderInMiddle = await _dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test/test");
			_dbContext.FileIndex.Remove(folderInMiddle);
			await _dbContext.SaveChangesAsync();

			await _query.AddParentItemsAsync("/test/test/test");
			var result = await _dbContext.FileIndex.ToListAsync();

			Assert.AreEqual(result.FirstOrDefault(p => p.FilePath == "/").FilePath, "/");
			Assert.AreEqual(result.FirstOrDefault(p => p.FilePath == "/test").FilePath, "/test");
			Assert.AreEqual(result.FirstOrDefault(p => p.FilePath == "/test/test").FilePath, "/test/test");
		}
	}
}
