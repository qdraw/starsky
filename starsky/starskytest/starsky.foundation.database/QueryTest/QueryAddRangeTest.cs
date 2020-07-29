using System.Collections.Generic;
using System.Linq;
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
	public class QueryAddRangeTest
	{
		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(QueryAddRangeTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task AddRangeAsync()
		{
			var expectedResult = new List<FileIndexItem>
			{
				new FileIndexItem {FileHash = "TEST4"},
				new FileIndexItem {FileHash = "TEST5"}
			};
			
			var serviceScopeFactory = CreateNewScope();
			var scope = serviceScopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			
			await new global::starsky.foundation.database.Query.Query(dbContext, new FakeMemoryCache(),
				new AppSettings {
					AddMemoryCache = false 
				}, serviceScopeFactory).AddRangeAsync(expectedResult);
			
			var queryFromDb = dbContext.FileIndex.Where(p => p.FileHash == "TEST4" || p.FileHash == "TEST5").ToList();
			Assert.AreEqual(expectedResult.FirstOrDefault().FileHash, queryFromDb.FirstOrDefault().FileHash);
			Assert.AreEqual(expectedResult[1].FileHash, queryFromDb[1].FileHash);
		}
		
		[TestMethod]
		public async Task AddRangeAsync_Disposed()
		{
			var expectedResult = new List<FileIndexItem>
			{
				new FileIndexItem {FileHash = "TEST4"},
				new FileIndexItem {FileHash = "TEST5"}
			};
			
			var serviceScopeFactory = CreateNewScope();
			var scope = serviceScopeFactory.CreateScope();
			var dbContextDisposed = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			
			// Dispose here
			await dbContextDisposed.DisposeAsync();
			
			await new global::starsky.foundation.database.Query.Query(dbContextDisposed, new FakeMemoryCache(),
				new AppSettings {
					AddMemoryCache = false 
				}, serviceScopeFactory).AddRangeAsync(expectedResult);
			
			var context = new InjectServiceScope(serviceScopeFactory).Context();
			var queryFromDb = context.FileIndex.Where(p => p.FileHash == "TEST4" || p.FileHash == "TEST5").ToList();

			Assert.AreEqual(expectedResult.FirstOrDefault().FileHash, queryFromDb.FirstOrDefault().FileHash);
			Assert.AreEqual(expectedResult[1].FileHash, queryFromDb[1].FileHash);
		}
		
		[TestMethod]
		public void AddRange()
		{
			var expectedResult = new List<FileIndexItem>
			{
				new FileIndexItem {FileHash = "TEST4"},
				new FileIndexItem {FileHash = "TEST5"}
			};
			
			var serviceScopeFactory = CreateNewScope();
			var scope = serviceScopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			
			new global::starsky.foundation.database.Query.Query(dbContext, new FakeMemoryCache(),
				new AppSettings {
					AddMemoryCache = false 
				}, serviceScopeFactory).AddRange(expectedResult);
			
			var queryFromDb = dbContext.FileIndex.Where(p => p.FileHash == "TEST4" || p.FileHash == "TEST5").ToList();
			Assert.AreEqual(expectedResult.FirstOrDefault().FileHash, queryFromDb.FirstOrDefault().FileHash);
			Assert.AreEqual(expectedResult[1].FileHash, queryFromDb[1].FileHash);
		}
		
	}
}
