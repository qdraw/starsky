using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Import;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.Import
{
	[TestClass]
	public class ImportQueryTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly ImportQuery _importQuery;
		private readonly ApplicationDbContext _dbContext;

		public ImportQueryTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase(nameof(ImportQueryTest));
			var options = builder.Options;
			_dbContext = new ApplicationDbContext(options);
			_importQuery = new ImportQuery(_dbContext);
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
			await _dbContext.ImportIndex.AddAsync(new ImportIndexItem
			{
				Status = ImportStatus.Ok, FileHash = "TEST2", AddToDatabase = DateTime.UtcNow,
			});
			await _dbContext.SaveChangesAsync();

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
	}
}
