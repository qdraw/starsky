using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public sealed class QueryAddParentItemsAsyncTest
	{
		private readonly Query? _query;
		private readonly ApplicationDbContext? _dbContext;

		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => 
				options.UseInMemoryDatabase(nameof(QueryAddParentItemsAsyncTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		public QueryAddParentItemsAsyncTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetRequiredService<IMemoryCache>();
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			_dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			_query = new Query(_dbContext, new AppSettings(), serviceScope, new FakeIWebLogger(),memoryCache);
		}

		[TestMethod]
		public async Task CheckIfHomeItemIsCreated()
		{
			if ( _query == null || _dbContext == null )
			{
				throw new WebException(
					"_query & _dbContext should not be null");
			}
			
			await _query.AddParentItemsAsync("/");
			var result = await _dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/");
			Assert.AreEqual("/", result?.FilePath);
			Assert.IsNotNull(result);
			
			_dbContext.FileIndex.Remove(result);
			await _dbContext.SaveChangesAsync();
		}
		
		[TestMethod]
		public async Task Check_If_Slash_Is_Created()
		{
			if ( _query == null || _dbContext == null )
			{
				throw new WebException(
					"_query & _dbContext should not be null");
			}
			
			await _query.AddParentItemsAsync("/test");
			var result = await _dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/");
			Assert.AreEqual("/", result?.FilePath);
			Assert.IsNotNull(result);
			
			_dbContext.FileIndex.Remove(result);
			await _dbContext.SaveChangesAsync();
		}
		
		[TestMethod]
		public async Task Check_If_Parent_Is_Created()
		{
			if ( _query == null || _dbContext == null )
			{
				throw new WebException(
					"_query & _dbContext should not be null");
			}
			
			await _query.AddParentItemsAsync("/test/file");
			var result = await _dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test");
			Assert.AreEqual("/test", result?.FilePath);
			Assert.IsNotNull(result);
			
			_dbContext.FileIndex.Remove(result);
			await _dbContext.SaveChangesAsync();
		}

		[TestMethod]
		public async Task MissingFolderIsAdded()
		{
			if ( _query == null || _dbContext == null )
			{
				throw new WebException(
					"_query & _dbContext should not be null");
			}
			
			await _query.AddParentItemsAsync("/test/test/test");
			
			var folderInMiddle = await _dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test/test");
			
			Assert.IsNotNull(folderInMiddle);
			
			_dbContext.FileIndex.Remove(folderInMiddle);
			await _dbContext.SaveChangesAsync();

			await _query.AddParentItemsAsync("/test/test/test");
			var result = await _dbContext.FileIndex.ToListAsync();

			Assert.AreEqual("/", result.Find(p => p.FilePath == "/")?.FilePath);
			Assert.AreEqual("/test", result.Find(p => p.FilePath == "/test")?.FilePath);
			Assert.AreEqual("/test/test", result.Find(p => p.FilePath == "/test/test")?.FilePath);
		}

		[TestMethod]
		public async Task AddParentItemsAsync_Home()
		{
			var dbContext = CreateNewScope().CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var query = new Query(dbContext,null!,null,null!);

			await query.AddParentItemsAsync("/test/test/test");

			var homeItem = dbContext.FileIndex.FirstOrDefault(p => p.FilePath == "/");
			Assert.AreEqual(string.Empty, homeItem?.ParentDirectory);
			Assert.AreEqual("/", homeItem?.FileName);

			var test1 = dbContext.FileIndex.FirstOrDefault(p => p.FilePath == "/test");
			Assert.AreEqual("/", test1?.ParentDirectory);
			Assert.AreEqual("test", test1?.FileName);

			var test2 = dbContext.FileIndex.FirstOrDefault(p => p.FilePath == "/test/test");
			Assert.AreEqual("/test", test2?.ParentDirectory);
			Assert.AreEqual("test", test2?.FileName);
		}
	}
}
