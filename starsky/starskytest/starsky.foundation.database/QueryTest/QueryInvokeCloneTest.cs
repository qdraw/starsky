using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public sealed class QueryInvokeCloneTest
	{
		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => 
				options.UseInMemoryDatabase(nameof(QueryInvokeCloneTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		[TestMethod]
		public async Task CloneInvokeTest()
		{
			var dbContext = CreateNewScope().CreateScope()
				.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var query = new Query(null,  
				new AppSettings(), CreateNewScope(), new FakeIWebLogger(),new FakeMemoryCache()).Clone(dbContext);
			
			Assert.IsNull(await query.GetSubPathByHashAsync("4444"));
		}

		[TestMethod]
		public async Task InvokeTest()
		{
			var dbContext = CreateNewScope().CreateScope()
				.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var query = new Query(null, 
				new AppSettings(), CreateNewScope(), new FakeIWebLogger(), new FakeMemoryCache());
			query.Invoke(dbContext);
			
			Assert.IsNull(await query.GetSubPathByHashAsync("4444"));
		}
	}
}
