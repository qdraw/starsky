using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Query;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public class QueryInvokeCloneTest
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
		public void CloneInvokeTest()
		{
			var dbContext = CreateNewScope().CreateScope()
				.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var query = new Query(null).Clone(dbContext);
			
			Assert.IsNull(query.GetSubPathByHash("4444"));
		}

		[TestMethod]
		public void InvokeTest()
		{
			var dbContext = CreateNewScope().CreateScope()
				.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var query = new Query(null);
			query.Invoke(dbContext);
			
			Assert.IsNull(query.GetSubPathByHash("4444"));
		}
	}
}
