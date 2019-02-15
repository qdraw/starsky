using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;
using starskycore.Services;

namespace starskytest.Helpers
{
	[TestClass]
	public class ConfigCliAppsStartupHelperTest
	{
		[TestMethod]
		public void ConfigCliAppsStartupHelperTestImportService()
		{
			var cliHelper = new ConfigCliAppsStartupHelper();

			var name = cliHelper.ExifTool().ToString();
			Assert.AreEqual(true,name.Contains(nameof(ExifTool)));
			
			name = cliHelper.SyncService().ToString();
			Assert.AreEqual(true,name.Contains(nameof(SyncService)));			
			
			name = cliHelper.ReadMeta().ToString();
			Assert.AreEqual(true,name.Contains(nameof(ReadMeta)));
			
			name = cliHelper.ImportService().ToString();
			Assert.AreEqual(true,name.Contains(nameof(ImportService)));		
		}
	}
}
