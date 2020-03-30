using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Serializer;

namespace starskytest.starsky.foundation.database.Serializer
{
	[TestClass]
	public class ExportDatabaseTest
	{
		private readonly ApplicationDbContext _dbContext;

		public ExportDatabaseTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase(nameof(ExportDatabaseTest));
			var options = builder.Options;
			_dbContext = new ApplicationDbContext(options);
		}

		[TestMethod]
		public async Task ExportTest()
		{
			await new ExportDatabase(_dbContext).Export();
		}
	}
}
