using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Models;
using starskycore.Services;

namespace starskytest.Services
{
	[TestClass]
	public class ExifToolTest
	{

		[TestMethod]
		public async Task ExifToolTest_()
		{
			var appSettings = new AppSettings
			{
				ExifToolPath = "/usr/local/bin/exiftool",
				StorageFolder = "/data/isight/__starsky/01-dif"
			};
			var subPathStorage = new StorageSubPathFilesystem(appSettings);

			var stream = await new ExifTool(subPathStorage, appSettings).WriteTagsAsync("/2018.01.01.17.00.01 kopie.jpg","-Software=\"Qdraw 2.0\"");
			
			
		}
	}
}
