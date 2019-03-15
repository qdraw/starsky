using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Data;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeMocks;
using starskytest.Models;

namespace starskytest.Services
{
	[TestClass]
	public class UpdateServiceTest
	{
		private readonly IMemoryCache _memoryCache;
		private IQuery _query;
		private AppSettings _appSettings;
		private FakeExiftool _exifTool;
		private ReadMeta _readMeta;

		public UpdateServiceTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase(nameof(UpdateService));
			var options = builder.Options;
			var dbContext = new ApplicationDbContext(options);
			_query = new Query(dbContext,_memoryCache);
			_appSettings = new AppSettings();
			_exifTool = new FakeExiftool();
			var fakeStorage = new FakeIStorage();
			_readMeta = new ReadMeta(fakeStorage,_appSettings,_memoryCache);
		}
		[TestMethod]
		public void Test()
		{
		}
		
		[TestMethod]
		public void Test1()
		{
//			new UpdateService(_query,_exifTool,_appSettings, _readMeta).Update(null,inputModel, fileIndexResultsList,collections,0);

		}

	}
}
