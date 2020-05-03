using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Import;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;

namespace starskytest.starsky.foundation.database.Import
{
	[TestClass]
	public class ImportQueryTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly ImportQuery _importQuery;
		private readonly ApplicationDbContext _dbContext;
		private readonly IServiceScopeFactory _serviceScope;

		public ImportQueryTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
            
			_serviceScope = CreateNewScope();
			var scope = _serviceScope.CreateScope();
			_dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			
			_importQuery = new ImportQuery(_serviceScope);
		}

		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(ImportQueryTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public void TestConnection_True()
		{
			var result =_importQuery.TestConnection();
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void TestConnection_Null()
		{
			var result = new ImportQuery(null).TestConnection();
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task IsHashInImportDbAsync_True()
		{
			var dbContext = new InjectServiceScope(null,_serviceScope).Context();

			await dbContext.ImportIndex.AddAsync(new ImportIndexItem
			{
				Status = ImportStatus.Ok, FileHash = "TEST2", AddToDatabase = DateTime.UtcNow,
			});
			await dbContext.SaveChangesAsync();

			var result = await _importQuery.IsHashInImportDbAsync("TEST2");
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public async Task IsHashInImportDbAsync_NotFound()
		{
			_dbContext.ImportIndex.Add(new ImportIndexItem
			{
				Status = ImportStatus.Ok, FileHash = "TEST", AddToDatabase = DateTime.UtcNow,
			});

			var result = await _importQuery.IsHashInImportDbAsync("Not-found");
			Assert.IsFalse(result);
		}

		[TestMethod]
		public async Task IsHashInImportDbAsync_ContextFail()
		{
			var result = await new ImportQuery(null).IsHashInImportDbAsync("TEST");
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public async Task AddAsync()
		{
			var expectedResult = new ImportIndexItem {FileHash = "TEST3"};
			var serviceScopeFactory = CreateNewScope();
			var scope = serviceScopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			
			await new ImportQuery(serviceScopeFactory).AddAsync(expectedResult);
			
			var queryFromDb = await dbContext.ImportIndex.FirstOrDefaultAsync(
				p => p.FileHash == expectedResult.FileHash);
			
			Assert.AreEqual(expectedResult.FileHash,queryFromDb.FileHash);
		}
		
		[TestMethod]
		public async Task History()
		{
			var expectedResult = new ImportIndexItem { AddToDatabase = DateTime.UtcNow, FileHash = "TEST8"};
			var serviceScopeFactory = CreateNewScope();
			
			await new ImportQuery(serviceScopeFactory).AddAsync(expectedResult);

			var historyResult = new ImportQuery(serviceScopeFactory).History();

			Assert.IsTrue(historyResult.Any(p => p.FileHash == "TEST8"));
		}
		
		[TestMethod]
		public async Task AddRangeAsync()
		{
			var expectedResult = new List<ImportIndexItem>
			{
				new ImportIndexItem {FileHash = "TEST4"},
				new ImportIndexItem {FileHash = "TEST5"}
			};
			var serviceScopeFactory = CreateNewScope();
			var scope = serviceScopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			
			await new ImportQuery(serviceScopeFactory).AddRangeAsync(expectedResult);
			
			var queryFromDb = dbContext.ImportIndex.Where(p => p.FileHash == "TEST4" || p.FileHash == "TEST5").ToList();
			Assert.AreEqual(expectedResult.FirstOrDefault().FileHash, queryFromDb.FirstOrDefault().FileHash);
			Assert.AreEqual(expectedResult[1].FileHash, queryFromDb[1].FileHash);
		}
		
		[TestMethod]
		public void AddRange()
		{
			var expectedResult = new List<ImportIndexItem>
			{
				new ImportIndexItem {FileHash = "TEST4"},
				new ImportIndexItem {FileHash = "TEST5"}
			};
			var serviceScopeFactory = CreateNewScope();
			var scope = serviceScopeFactory.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			
			new ImportQuery(serviceScopeFactory).AddRange(expectedResult);
			
			var queryFromDb = dbContext.ImportIndex.Where(p => p.FileHash == "TEST4" || p.FileHash == "TEST5").ToList();
			Assert.AreEqual(expectedResult.FirstOrDefault().FileHash, queryFromDb.FirstOrDefault().FileHash);
			Assert.AreEqual(expectedResult[1].FileHash, queryFromDb[1].FileHash);
		}
		
	}
}
