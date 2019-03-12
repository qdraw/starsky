using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Data;
using starskycore.Models;
using starskycore.Services;

namespace starskytest.Services
{
	[TestClass]
	public class ReplaceServiceTest
	{
		
		private ReplaceService _replace;
		private readonly Query _query;
		private readonly IMemoryCache _memoryCache;

		public ReplaceServiceTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase(nameof(ReplaceService));
			var options = builder.Options;
			var dbContext = new ApplicationDbContext(options);
			_query = new Query(dbContext,_memoryCache);
			_replace = new ReplaceService(_query,new AppSettings());

		}
		

	}
}
