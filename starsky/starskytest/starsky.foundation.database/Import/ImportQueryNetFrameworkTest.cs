using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Import;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;

namespace starskytest.starsky.foundation.database.Import
{
	[TestClass]
	public class ImportQueryNetFrameworkTest
	{
		private readonly ImportQuery _importQueryNetFramework;
		private readonly ApplicationDbContext _dbContext;
		private readonly IServiceScopeFactory _serviceScope;

		public ImportQueryNetFrameworkTest()
		{
			_serviceScope = CreateNewScope();
			var scope = _serviceScope.CreateScope();
			_dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			
			_importQueryNetFramework = new ImportQueryNetFramework(_serviceScope);
		}

		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(ImportQueryNetFrameworkTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task IsHashInImportDbAsync_True()
		{
			var dbContext = new InjectServiceScope(_serviceScope).Context();

			await dbContext.ImportIndex.AddAsync(new ImportIndexItem
			{
				Status = ImportStatus.Ok, FileHash = "TEST2", AddToDatabase = DateTime.UtcNow,
			});
			await dbContext.SaveChangesAsync();

			var result = await _importQueryNetFramework.IsHashInImportDbAsync("TEST2");
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public async Task IsHashInImportDbAsync_NotFound()
		{
			_dbContext.ImportIndex.Add(new ImportIndexItem
			{
				Status = ImportStatus.Ok, FileHash = "TEST", AddToDatabase = DateTime.UtcNow,
			});

			var result = await _importQueryNetFramework.IsHashInImportDbAsync("Not-found");
			Assert.IsFalse(result);
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
	}
}
