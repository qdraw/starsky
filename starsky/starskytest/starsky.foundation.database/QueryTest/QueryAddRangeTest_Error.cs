using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public class QueryAddRangeTest_Error
{
	private class SqliteExceptionDbContext : ApplicationDbContext
	{
		public SqliteExceptionDbContext(DbContextOptions options) : base(options)
		{
		}

		public int Count { get; set; }


#pragma warning disable 8603
		public override DbSet<FileIndexItem> FileIndex => null;
#pragma warning restore 8603

		public override int SaveChanges()
		{
			Count++;
			if ( Count == 1 )
			{
				throw new Microsoft.Data.Sqlite.SqliteException("t",1,2);
			}
			return Count;
		}	
			
		public override Task<int> SaveChangesAsync(
			CancellationToken cancellationToken = default)
		{
			Count++;
			if ( Count == 1 )
			{
				throw new Microsoft.Data.Sqlite.SqliteException("t",1,2);
			}
			return Task.FromResult(Count);
		}
	}
	
	private static IServiceScopeFactory CreateNewScopeSqliteException()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(QueryTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}
	
	[TestMethod]
	public async Task AddRangeAsync_SQLiteException()
	{
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase("MovieListDatabase")
			.Options;

		var scope = CreateNewScopeSqliteException();

		var sqLiteFailContext = new SqliteExceptionDbContext(options);
		Assert.AreEqual(0,sqLiteFailContext.Count);

		var fakeQuery = new Query(sqLiteFailContext, new AppSettings(), scope, new FakeIWebLogger());
		await fakeQuery.AddRangeAsync(new List<FileIndexItem>{new FileIndexItem("/test22.jpg")});
			
		Assert.AreEqual(1, sqLiteFailContext.Count);
	}

}
