using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;

namespace starskytests.Helpers
{
	[TestClass]
	public class ConfigCliAppsStartupHelperTest
	{
		[TestMethod]
		public void ConfigCliAppsStartupHelperTestImportService()
		{
			var cliHelper = new ConfigCliAppsStartupHelper();

			var name = cliHelper.ExifTool().ToString();
			Assert.AreEqual(true,name.Contains(nameof(starsky.Services.ExifTool)));
			
			name = cliHelper.SyncService().ToString();
			Assert.AreEqual(true,name.Contains(nameof(starsky.Services.SyncService)));			
			
			name = cliHelper.ReadMeta().ToString();
			Assert.AreEqual(true,name.Contains(nameof(starsky.Services.ReadMeta)));
			
			name = cliHelper.ImportService().ToString();
			Assert.AreEqual(true,name.Contains(nameof(starsky.Services.ImportService)));		
		}
	}
}
