using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public class QueryGetAllRecursive_Error
{
	private IServiceScopeFactory _serviceScopeFactory;

	private void CreateScope()
	{
		var services = new ServiceCollection();
		services.AddSingleton<AppSettings>();

		services.AddSingleton<IWebLogger, FakeIWebLogger>();
		services.AddDbContext<ApplicationDbContext>(options => 
			options.UseInMemoryDatabase(nameof(CreateScope)));
			
		var serviceProvider = services.BuildServiceProvider();
		_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}
	
	public static int InvalidOperationExceptionDbContextCount { get; set; }
		
	private class InvalidOperationExceptionDbContext : ApplicationDbContext
	{
		public InvalidOperationExceptionDbContext(DbContextOptions options) : base(options)
		{
		}

		public override DbSet<FileIndexItem> FileIndex
		{
			get
			{
				InvalidOperationExceptionDbContextCount++;
				throw new InvalidOperationException("from test");
			}
			set
			{
			}
		}
	}
	
	[TestMethod]
	public async Task GetAllRecursiveAsync_InvalidOperationExceptionDbContext()
	{
		InvalidOperationExceptionDbContextCount = 0;
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "MovieListDatabase")
			.Options;
			
		CreateScope();
		var fakeQuery = new Query(new InvalidOperationExceptionDbContext(options), 
			null!, _serviceScopeFactory! ,new FakeIWebLogger());
		await fakeQuery.GetAllRecursiveAsync("test");
			
		Assert.IsTrue(InvalidOperationExceptionDbContextCount == 1);
	}
}
