using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;

namespace starskytest.starsky.foundation.database.Helpers
{
	[TestClass]
	public class RunMigrationsTest
	{
		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task Test()
		{
			IServiceCollection services = new ServiceCollection();
			var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();
			services
				.AddDbContext<ApplicationDbContext>(b =>
					b.UseInMemoryDatabase("test1234").UseInternalServiceProvider(efServiceProvider));
			var serviceProvider = services.BuildServiceProvider();
			var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

			await RunMigrations.Run(serviceScopeFactory.CreateScope());
			// expect exception: Relational-specific methods can only be used when the context is using a relational database provider.
		}
	}
}
