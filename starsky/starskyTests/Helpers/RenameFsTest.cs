using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Data;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;

namespace starskytests.Helpers
{
	[TestClass]
	public class RenameFsTest
	{
		private readonly IMemoryCache _memoryCache;
		private Query _query;
		private AppSettings _appSettings;
		private CreateAnImage _newImage;

		public RenameFsTest()
		{
			var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
			
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase("test");
			var options = builder.Options;
			var context = new ApplicationDbContext(options);
			
			_newImage = new CreateAnImage();

			_appSettings = new AppSettings
			{
				StorageFolder = _newImage.BasePath,
				ThumbnailTempFolder = _newImage.BasePath
			};
			_query = new Query(context,_memoryCache, _appSettings);
			_query.AddItem(new FileIndexItem
			{
				FileName = _newImage.FileName,
				ParentDirectory = "/"
			});
		}

		[TestMethod]
		public void RenameFsTest_WithoutAnyItems()
		{
			var renameFs = new RenameFs(_appSettings, _query).Rename("/non-exist.jpg", "/non-exist2.jpg");
			Assert.AreEqual(renameFs.FirstOrDefault().Status,FileIndexItem.ExifStatus.NotFoundNotInIndex);
		}
		
		[TestMethod]
		public void RenameFsTest_Items()
		{
			var renameFs = new RenameFs(_appSettings, _query).Rename(_newImage.DbPath, "/test2.jpg");

			Assert.AreEqual(1,renameFs.Count);
		}
	}
}
